using Grpc.Core;
using Acderator.Shared;
using Isopoh.Cryptography.Argon2;
using MagicOnion.Server;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Acderator.Server;

public enum AppError
{
    BadRequest = 10001,
    InvalidRequestFormat = 10002,
    Unauthorized = 11001,
    Forbidden = 11002,
    NotFound = 12001,
    Conflict = 12002,
    PreconditionFailed = 13001,
    Maintenance = 13002,
    UnsupportedMediaType = 14001,
    TooManyRequests = 14002,
    PayloadTooLarge = 14003,
    ServiceFailure = 15000,
    ResponseFailure = 15001,
    DataFailure = 15002,
    ResourceFailure = 15003,
}

internal static class AppErrorMessages
{
    public static string Get(AppError error) => error switch
    {
        AppError.BadRequest or AppError.InvalidRequestFormat => "リクエストが正しくありません",
        AppError.Unauthorized => "認証に失敗しました",
        AppError.Forbidden => "この操作は許可されていません",
        AppError.NotFound => "対象データが見つかりません",
        AppError.Conflict => "リクエストを処理できませんでした",
        AppError.PreconditionFailed => "データの更新が必要です",
        AppError.Maintenance => "現在メンテナンス中です",
        AppError.UnsupportedMediaType => "対応していないリクエスト形式です",
        AppError.TooManyRequests => "しばらく時間をおいてから再度お試しください",
        AppError.PayloadTooLarge => "リクエストが大きすぎます",
        _ => "サーバーエラーが発生しました",
    };
}

public abstract class AcderatorServiceBase<TService> : ServiceBase<TService>
    where TService : class, MagicOnion.IService<TService>
{
    private readonly IOptions<ServerOptions> options;
    private readonly ILogger logger;

    protected AcderatorServiceBase(IOptions<ServerOptions> options, ILogger logger)
    {
        this.options = options;
        this.logger = logger;
    }

    protected async Task<TResponse> ExecuteAsync<TResponse>(Func<Task<TResponse>> action)
        where TResponse : ResponseBase, new()
    {
        if (options.Value.MaintenanceMode) return Failure<TResponse>(AppError.Maintenance);

        try
        {
            return await action();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled RPC failure");
            return Failure<TResponse>(AppError.ServiceFailure);
        }
    }

    protected string? Header(string key) => Context.CallContext.RequestHeaders
        .FirstOrDefault(entry => string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase))?.Value;

    protected TResponse Failure<TResponse>(AppError error, Dictionary<string, object>? master = null)
        where TResponse : ResponseBase, new() => new()
        {
            Header = new ResponseHeader
            {
                Code = (int)error,
                Message = AppErrorMessages.Get(error),
                Master = master ?? new(),
            },
        };
}

public sealed class TokenService
{
    internal string Generate() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    internal string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    internal bool IsValid(string? token) => token is { Length: 64 } && token.All(Uri.IsHexDigit);
}

public sealed class PasswordService
{
    internal string Hash(string password) => Argon2.Hash(password);

    internal PasswordMatch Verify(string stored, string candidate)
    {
        return stored switch
        {
            _ when stored.StartsWith("$argon2", StringComparison.Ordinal) => Argon2.Verify(stored, candidate) ? PasswordMatch.Current : PasswordMatch.Invalid,
            _ => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(candidate)) ? PasswordMatch.Legacy : PasswordMatch.Invalid,
        };
    }
}

internal enum PasswordMatch
{
    Invalid,
    Current,
    Legacy,
}
