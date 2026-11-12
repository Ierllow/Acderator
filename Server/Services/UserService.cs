using Acderator.Shared;
using MagicOnion;
using Microsoft.Extensions.Options;

namespace Acderator.Server.Services;

public sealed class UserService(GameRepository repository, TokenService tokens, IOptions<ServerOptions> options, ILogger<UserService> logger)
    : AcderatorServiceBase<IUserService>(options, logger), IUserService
{
    public async UnaryResult<UserDataResponse> GetDataAsync(UserDataRequest request) => await ExecuteAsync(async () =>
    {
        var authentication = await Authentication.AuthenticateAsync(Header("authorization"), repository, tokens);
        if (authentication is null) return Failure<UserDataResponse>(AppError.Unauthorized);
        return new UserDataResponse
        {
            Scores = await repository.GetScoresAsync(authentication.Value.UserId),
        };
    });
}
