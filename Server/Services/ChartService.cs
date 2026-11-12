using Acderator.Shared;
using MagicOnion;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Acderator.Server.Services;

public sealed class ChartService(IHostEnvironment environment, GameRepository repository, TokenService tokens, IOptions<ServerOptions> options, ILogger<ChartService> logger)
    : AcderatorServiceBase<IChartService>(options, logger), IChartService
{
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly string chartDataDirectory = Path.IsPathRooted(options.Value.ChartDataDirectory)
        ? options.Value.ChartDataDirectory
        : Path.Combine(environment.ContentRootPath, options.Value.ChartDataDirectory);
    private readonly ConcurrentDictionary<int, ChartResponse> chartCache = new();

    public async UnaryResult<ChartResponse> GetAsync(ChartRequest request) => await ExecuteAsync(async () =>
    {
        var authentication = await Authentication.AuthenticateAsync(Header("authorization"), repository, tokens);
        if (authentication is null) return Failure<ChartResponse>(AppError.Unauthorized);

        if (chartCache.TryGetValue(request.Sid, out var cached)) return cached;

        var path = Path.Combine(chartDataDirectory, $"{request.Sid}.json");
        if (!File.Exists(path)) return Failure<ChartResponse>(AppError.NotFound);
        await using var stream = File.OpenRead(path);
        var response = await JsonSerializer.DeserializeAsync<ChartResponse>(stream, jsonOptions);
        if (response is null) return Failure<ChartResponse>(AppError.InvalidRequestFormat);
        return chartCache[request.Sid] = response;
    });
}
