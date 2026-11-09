using Acderator.Shared;
using MagicOnion;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Acderator.Server.Services;

public sealed class ChartService : AcderatorServiceBase<IChartService>, IChartService
{
    private readonly IHostEnvironment environment;
    private readonly GameRepository repository;
    private readonly TokenService tokens;
    private readonly ServerOptions options;

    public ChartService(IHostEnvironment environment, GameRepository repository, TokenService tokens, IOptions<ServerOptions> options, ILogger<ChartService> logger)
        : base(options, logger)
    {
        this.environment = environment;
        this.repository = repository;
        this.tokens = tokens;
        this.options = options.Value;
    }

    public async UnaryResult<ChartResponse> GetAsync(ChartRequest request) => await ExecuteAsync(async () =>
    {
        var authentication = await Authentication.AuthenticateAsync(Header("authorization"), repository, tokens);
        if (authentication is null) return Failure<ChartResponse>(AppError.Unauthorized);

        var path = Path.Combine(Path.IsPathRooted(options.ChartDataDirectory) ? options.ChartDataDirectory : Path.Combine(environment.ContentRootPath, options.ChartDataDirectory), $"{request.Sid}.json");
        if (!File.Exists(path)) return Failure<ChartResponse>(AppError.NotFound);
        var response = JsonSerializer.Deserialize<ChartResponse>(await File.ReadAllTextAsync(path), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        });
        if (response is null) return Failure<ChartResponse>(AppError.InvalidRequestFormat);
        return response;
    });
}
