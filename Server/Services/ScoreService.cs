using Acderator.Shared;
using MagicOnion;
using Microsoft.Extensions.Options;

namespace Acderator.Server.Services;

public sealed class ScoreService : AcderatorServiceBase<IScoreService>, IScoreService
{
    private readonly GameRepository repository;
    private readonly TokenService tokens;

    public ScoreService(GameRepository repository, TokenService tokens, IOptions<ServerOptions> options, ILogger<ScoreService> logger)
        : base(options, logger)
    {
        this.repository = repository;
        this.tokens = tokens;
    }

    public async UnaryResult<ScoreBeginResponse> BeginAsync(ScoreBeginRequest request) => await ExecuteAsync(async () =>
    {
        var authentication = await Authentication.AuthenticateAsync(Header("authorization"), repository, tokens);
        if (authentication is null) return Failure<ScoreBeginResponse>(AppError.Unauthorized);
        var sessionToken = tokens.Generate();
        if (!await repository.BeginScoreAsync(authentication.Value.UserId, request.ScoreId, tokens.Hash(sessionToken)))
        {
            return Failure<ScoreBeginResponse>(AppError.BadRequest);
        }
        return new ScoreBeginResponse { SessionId = sessionToken };
    });

    public async UnaryResult<ScoreSubmitResponse> SubmitAsync(ScoreSubmitRequest request) => await ExecuteAsync(async () =>
    {
        var authentication = await Authentication.AuthenticateAsync(Header("authorization"), repository, tokens);
        if (authentication is null) return Failure<ScoreSubmitResponse>(AppError.Unauthorized);
        if (!tokens.IsValid(request.SessionId)) return Failure<ScoreSubmitResponse>(AppError.NotFound);

        return await repository.SubmitScoreAsync(authentication.Value.UserId, tokens.Hash(request.SessionId), request.Score) switch
        {
            SubmitScoreResult.Success => new(),
            SubmitScoreResult.InvalidScore => Failure<ScoreSubmitResponse>(AppError.BadRequest),
            SubmitScoreResult.SessionNotFound => Failure<ScoreSubmitResponse>(AppError.NotFound),
            _ => Failure<ScoreSubmitResponse>(AppError.ServiceFailure),
        };
    });
}
