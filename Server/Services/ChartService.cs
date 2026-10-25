using Acderator.Shared;
using MagicOnion;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Acderator.Server.Services;

public sealed class ChartService : AcderatorServiceBase<IChartService>, IChartService
{
    private readonly IHostEnvironment environment;
    private readonly ServerOptions options;

    public ChartService(IHostEnvironment environment, IOptions<ServerOptions> options, ILogger<ChartService> logger)
        : base(options, logger)
    {
        this.environment = environment;
        this.options = options.Value;
    }

    public async UnaryResult<ChartResponse> GetAsync(ChartRequest request) => await ExecuteAsync(async () =>
    {
        var path = Path.Combine(Path.IsPathRooted(options.ChartDataDirectory) ? options.ChartDataDirectory : Path.Combine(environment.ContentRootPath, options.ChartDataDirectory), $"{request.Sid}.json");
        if (!File.Exists(path)) return Failure<ChartResponse>(AppError.NotFound);
        var response = JsonSerializer.Deserialize<ChartResponse>(await File.ReadAllTextAsync(path), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        });
        if (response is null) return Failure<ChartResponse>(AppError.InvalidRequestFormat);
        response.Header = new ResponseHeader();
        return response;
    });
}
