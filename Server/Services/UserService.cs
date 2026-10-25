using Acderator.Shared;
using MagicOnion;
using Microsoft.Extensions.Options;

namespace Acderator.Server.Services;

public sealed class UserService : AcderatorServiceBase<IUserService>, IUserService
{
    private readonly GameRepository repository;
    private readonly TokenService tokens;

    public UserService(GameRepository repository, TokenService tokens, IOptions<ServerOptions> options, ILogger<UserService> logger)
        : base(options, logger)
    {
        this.repository = repository;
        this.tokens = tokens;
    }

    public async UnaryResult<UserDataResponse> GetDataAsync(UserDataRequest request) => await ExecuteAsync(async () =>
    {
        var authentication = await Authentication.AuthenticateAsync(Header("authorization"), repository, tokens);
        if (authentication is null) return Failure<UserDataResponse>(AppError.Unauthorized);
        return new UserDataResponse
        {
            Header = new ResponseHeader(),
            Scores = await repository.GetScoresAsync(authentication.Value.UserId),
        };
    });
}
