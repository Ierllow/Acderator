using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Acderator.Server;

public sealed class MasterDataStore(IDbContextFactory<AcderatorDbContext> contextFactory, IHostEnvironment environment, IOptions<ServerOptions> options, ILogger<MasterDataStore> logger)
{
    private readonly JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private readonly IDbContextFactory<AcderatorDbContext> contextFactory = contextFactory;
    private readonly IHostEnvironment environment = environment;
    private readonly ServerOptions options = options.Value;
    private readonly ILogger<MasterDataStore> logger = logger;
    private JsonDocument document = JsonDocument.Parse("{}");
    private Dictionary<string, object> payload = [];
    private string version = string.Empty;

    internal Dictionary<string, object> Payload => payload;

    internal string Version => version;

    internal async Task SynchronizeAsync()
    {
        var configuredPath = options.MasterDataPath;
        var path = Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(environment.ContentRootPath, configuredPath);
        var nextDocument = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        var root = nextDocument.RootElement;
        var nextPayload = (Dictionary<string, object>)ConvertElement(root);
        var hasVersion = root.TryGetProperty("version_master", out var versionElement);
        var nextVersion = hasVersion ? versionElement.GetString() ?? string.Empty : string.Empty;

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();
            await DeleteMasterDataAsync(context);
            context.MasterVersions.Add(new MasterVersionEntity { Version = nextVersion });
            context.TitleMasters.AddRange(ReadEntities<TitleMasterEntity>(root, "title_masters"));
            context.SongSelectMasters.AddRange(ReadEntities<SongSelectMasterEntity>(root, "song_select_masters"));
            context.SongMasters.AddRange(ReadEntities<SongMasterEntity>(root, "song_masters"));
            context.SongScoreRateMasters.AddRange(ReadEntities<SongScoreRateMasterEntity>(root, "score_rate_masters"));
            context.SongBaseScoreMasters.AddRange(ReadEntities<SongBaseScoreMasterEntity>(root, "base_score_masters"));
            context.SongJudgeZoneMasters.AddRange(ReadEntities<SongJudgeZoneMasterEntity>(root, "judge_zone_masters"));
            context.SongBaseHpMasters.AddRange(ReadEntities<SongBaseHpMasterEntity>(root, "base_hp_masters"));
            context.SongHpRateMasters.AddRange(ReadEntities<SongHpRateMasterEntity>(root, "hp_rate_masters"));
            context.SoundSheetNameMasters.AddRange(ReadEntities<SoundSheetNameMasterEntity>(root, "sound_sheet_masters"));
            context.ResultMasters.AddRange(ReadEntities<ResultMasterEntity>(root, "result_masters"));
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            nextDocument.Dispose();
            throw;
        }

        document.Dispose();
        document = nextDocument;
        payload = nextPayload;
        version = nextVersion;
        logger.LogInformation("Synchronized master data version {Version}", Version);
    }

    internal bool IsCurrentVersion(string? clientVersion) => string.Equals(clientVersion ?? string.Empty, Version, StringComparison.Ordinal);

    private async Task DeleteMasterDataAsync(AcderatorDbContext context)
    {
        await context.MasterVersions.ExecuteDeleteAsync();
        await context.TitleMasters.ExecuteDeleteAsync();
        await context.SongSelectMasters.ExecuteDeleteAsync();
        await context.SongMasters.ExecuteDeleteAsync();
        await context.SongScoreRateMasters.ExecuteDeleteAsync();
        await context.SongBaseScoreMasters.ExecuteDeleteAsync();
        await context.SongJudgeZoneMasters.ExecuteDeleteAsync();
        await context.SongBaseHpMasters.ExecuteDeleteAsync();
        await context.SongHpRateMasters.ExecuteDeleteAsync();
        await context.SoundSheetNameMasters.ExecuteDeleteAsync();
        await context.ResultMasters.ExecuteDeleteAsync();
    }

    private List<TEntity> ReadEntities<TEntity>(JsonElement root, string propertyName) where TEntity : class
    {
        if (!root.TryGetProperty(propertyName, out var array)) return [];
        return array.Deserialize<List<TEntity>>(jsonOptions) ?? [];
    }

    private object ConvertElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject().ToDictionary(x => x.Name, x => ConvertElement(x.Value)),
        JsonValueKind.Array => element.EnumerateArray().Select(ConvertElement).ToList(),
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
        JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => string.Empty,
    };
}
