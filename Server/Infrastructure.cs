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

public abstract class AcderatorServiceBase<TService>(IOptions<ServerOptions> options, ILogger logger) : ServiceBase<TService>
    where TService : class, MagicOnion.IService<TService>
{
    private readonly ServerOptions serverOptions = options.Value;

    protected async Task<TResponse> ExecuteAsync<TResponse>(Func<Task<TResponse>> action)
        where TResponse : ResponseBase, new()
    {
        if (serverOptions.MaintenanceMode) return Failure<TResponse>(AppError.Maintenance);

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

    protected string? Header(string key)
    {
        foreach (var entry in Context.CallContext.RequestHeaders)
        {
            if (string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase)) return entry.Value;
        }
        return null;
    }

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
    internal string Generate()
    {
        Span<byte> token = stackalloc byte[32];
        RandomNumberGenerator.Fill(token);
        return ToLowerHex(token);
    }

    internal string Hash(ReadOnlySpan<char> token)
    {
        var byteCount = Encoding.UTF8.GetByteCount(token);
        Span<byte> tokenBytes = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];
        Encoding.UTF8.GetBytes(token, tokenBytes);
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(tokenBytes, hash);
        return ToLowerHex(hash);
    }

    internal bool IsValid(string? token) => token is not null && IsValid(token.AsSpan());

    internal bool IsValid(ReadOnlySpan<char> token)
    {
        if (token.Length != 64) return false;
        foreach (var character in token)
        {
            if (!Uri.IsHexDigit(character)) return false;
        }
        return true;
    }

    private string ToLowerHex(ReadOnlySpan<byte> bytes)
    {
        const string hex = "0123456789abcdef";
        Span<char> characters = stackalloc char[bytes.Length * 2];
        for (var index = 0; index < bytes.Length; index++)
        {
            var value = bytes[index];
            characters[index * 2] = hex[value >> 4];
            characters[index * 2 + 1] = hex[value & 0x0F];
        }
        return new string(characters);
    }
}

public sealed class PasswordService
{
    internal string Hash(string password) => Argon2.Hash(password);

    internal PasswordMatch Verify(string stored, string candidate)
    {
        if (stored.StartsWith("$argon2", StringComparison.Ordinal)) return Argon2.Verify(stored, candidate) ? PasswordMatch.Current : PasswordMatch.Invalid;
        return FixedTimeEquals(stored, candidate) ? PasswordMatch.Legacy : PasswordMatch.Invalid;
    }

    private bool FixedTimeEquals(string stored, string candidate)
    {
        var storedByteCount = Encoding.UTF8.GetByteCount(stored);
        if (storedByteCount != Encoding.UTF8.GetByteCount(candidate)) return false;

        Span<byte> storedBytes = storedByteCount <= 256 ? stackalloc byte[storedByteCount] : new byte[storedByteCount];
        Span<byte> candidateBytes = storedByteCount <= 256 ? stackalloc byte[storedByteCount] : new byte[storedByteCount];
        Encoding.UTF8.GetBytes(stored, storedBytes);
        Encoding.UTF8.GetBytes(candidate, candidateBytes);
        return CryptographicOperations.FixedTimeEquals(storedBytes, candidateBytes);
    }
}

internal enum PasswordMatch
{
    Invalid,
    Current,
    Legacy,
}
