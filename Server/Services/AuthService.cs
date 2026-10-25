using Acderator.Shared;
using MagicOnion;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Acderator.Server.Services;

public sealed class AuthService : AcderatorServiceBase<IAuthService>, IAuthService
{
    private readonly GameRepository repository;
    private readonly MasterDataStore masterData;
    private readonly PasswordService passwords;
    private readonly TokenService tokens;
    private readonly ServerOptions options;

    public AuthService(GameRepository repository, MasterDataStore masterData, PasswordService passwords, TokenService tokens, IOptions<ServerOptions> options, ILogger<AuthService> logger)
        : base(options, logger)
    {
        this.repository = repository;
        this.masterData = masterData;
        this.passwords = passwords;
        this.tokens = tokens;
        this.options = options.Value;
    }

    public async UnaryResult<LoginResponse> LoginAsync(LoginRequest request) => await ExecuteAsync(async () =>
    {
        if (!masterData.IsCurrentVersion(Header("x-master-version")))
        {
            return Failure<LoginResponse>(AppError.PreconditionFailed, masterData.Payload);
        }
        if (string.IsNullOrEmpty(request.UserId) || request.UserId.Length > 255 || Encoding.UTF8.GetByteCount(request.Password) > options.MaxPasswordBytes)
        {
            return Failure<LoginResponse>(AppError.Unauthorized);
        }

        var user = await repository.FindUserAsync(request.UserId);
        if (user is null) return Failure<LoginResponse>(AppError.Unauthorized);
        var match = passwords.Verify(user.Password, request.Password);
        if (match == PasswordMatch.Invalid) return Failure<LoginResponse>(AppError.Unauthorized);

        var token = tokens.Generate();
        await repository.CreateLoginSessionAsync(request.UserId, tokens.Hash(token), match == PasswordMatch.Legacy ? passwords.Hash(request.Password) : null);
        return new LoginResponse { Header = Success(token) };
    });

    public async UnaryResult<RegisterResponse> RegisterAsync(RegisterRequest request) => await ExecuteAsync(async () =>
    {
        if (!masterData.IsCurrentVersion(Header("x-master-version")))
        {
            return Failure<RegisterResponse>(AppError.PreconditionFailed, masterData.Payload);
        }
        if (string.IsNullOrWhiteSpace(request.Uuid) || Encoding.UTF8.GetByteCount(request.Uuid) > options.MaxUuidBytes)
        {
            return Failure<RegisterResponse>(AppError.BadRequest);
        }
        if (await repository.UuidExistsAsync(request.Uuid)) return Failure<RegisterResponse>(AppError.Conflict);

        var userId = RandomNumberGenerator.GetInt32(100_000_000, 1_000_000_000);
        var password = userId.ToString();
        var token = tokens.Generate();
        if (!await repository.CreateUserAndSessionAsync(userId, request.Uuid, passwords.Hash(password), tokens.Hash(token)))
        {
            return Failure<RegisterResponse>(AppError.Conflict);
        }
        return new RegisterResponse
        {
            Header = Success(token),
            Token = token,
            UserId = userId,
            Password = password,
        };
    });

    public async UnaryResult<LogoutResponse> LogoutAsync(LogoutRequest request) => await ExecuteAsync(async () =>
    {
        var authentication = await Authentication.AuthenticateAsync(Header("authorization"), repository, tokens);
        if (authentication is null) return Failure<LogoutResponse>(AppError.Unauthorized);
        await repository.LogoutAsync(authentication.Value.SessionHash);
        return new LogoutResponse { Header = Success() };
    });

    private ResponseHeader Success(string token = "") => new() { Code = 0, Token = token };
}
