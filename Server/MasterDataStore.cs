using Dapper;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Text.Json;

namespace Acderator.Server;

public sealed class MasterDataStore
{
    private readonly MySqlDataSource dataSource;
    private readonly IHostEnvironment environment;
    private readonly ServerOptions options;
    private readonly ILogger<MasterDataStore> logger;
    private JsonDocument document = JsonDocument.Parse("{}");

    public MasterDataStore(MySqlDataSource dataSource, IHostEnvironment environment, IOptions<ServerOptions> options, ILogger<MasterDataStore> logger)
    {
        this.dataSource = dataSource;
        this.environment = environment;
        this.options = options.Value;
        this.logger = logger;
    }

    internal Dictionary<string, object> Payload => (Dictionary<string, object>)ConvertElement(document.RootElement);

    internal string Version => document.RootElement.TryGetProperty("version_master", out var version) ? version.GetString() ?? string.Empty : string.Empty;

    internal async Task SynchronizeAsync()
    {
        document.Dispose();
        document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.IsPathRooted(options.MasterDataPath) ? options.MasterDataPath : Path.Combine(environment.ContentRootPath, options.MasterDataPath)));

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        foreach (var table in new[]
        {
            "master_version", "title_master", "song_select_master", "song_master",
            "song_score_rate_master", "song_base_score_master", "song_judge_zone_master",
            "song_base_hp_master", "song_hp_rate_master", "sound_sheet_name_master", "result_master",
        })
        {
            await connection.ExecuteAsync($"DELETE FROM {table}", transaction: transaction);
        }

        await connection.ExecuteAsync("INSERT INTO master_version (version) VALUES (@Version)", new { Version }, transaction);
        await InsertArray(connection, transaction, "title_masters", "INSERT INTO title_master (tid) VALUES (@tid)");
        await InsertArray(connection, transaction, "song_select_masters", "INSERT INTO song_select_master (`group`, start_song_time, song_time) VALUES (@group, @start_song_time, @song_time)");
        await InsertArray(connection, transaction, "song_masters", "INSERT INTO song_master (sid, `group`, difficulty, name, composer, start_offset, bg) VALUES (@sid, @group, @difficulty, @name, @composer, @start_offset, @bg)");
        await InsertArray(connection, transaction, "score_rate_masters", "INSERT INTO song_score_rate_master (r_type, rate) VALUES (@r_type, @rate)");
        await InsertArray(connection, transaction, "base_score_masters", "INSERT INTO song_base_score_master (score) VALUES (@score)");
        await InsertArray(connection, transaction, "judge_zone_masters", "INSERT INTO song_judge_zone_master (j_type, zone) VALUES (@j_type, @zone)");
        await InsertArray(connection, transaction, "base_hp_masters", "INSERT INTO song_base_hp_master (hp) VALUES (@hp)");
        await InsertArray(connection, transaction, "hp_rate_masters", "INSERT INTO song_hp_rate_master (j_type, rate) VALUES (@j_type, @rate)");
        await InsertArray(connection, transaction, "sound_sheet_masters", "INSERT INTO sound_sheet_name_master (category, id, sheet_name, cue_name) VALUES (@category, @id, @sheet_name, @cue_name)");
        await InsertArray(connection, transaction, "result_masters", "INSERT INTO result_master (rid) VALUES (@rid)");
        await transaction.CommitAsync();
        logger.LogInformation("Synchronized master data version {Version}", Version);
    }

    internal bool IsCurrentVersion(string? clientVersion) =>
        string.Equals(clientVersion ?? string.Empty, Version, StringComparison.Ordinal);

    private async Task InsertArray(MySqlConnection connection, MySqlTransaction transaction, string propertyName, string sql)
    {
        if (!document.RootElement.TryGetProperty(propertyName, out var array)) return;
        foreach (var item in array.EnumerateArray())
        {
            await connection.ExecuteAsync(sql, ToParameters(item), transaction);
        }
    }

    private DynamicParameters ToParameters(JsonElement element)
    {
        var parameters = new DynamicParameters();
        foreach (var property in element.EnumerateObject()) parameters.Add(property.Name, ConvertElement(property.Value));
        return parameters;
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
