namespace Acderator.Server.Services;

internal static class Authentication
{
    public static async Task<(string UserId, string SessionHash)?> AuthenticateAsync(string? authorization, GameRepository repository, TokenService tokens)
    {
        if (!TryHashBearerToken(authorization, tokens, out var sessionHash)) return null;
        var userId = await repository.AuthenticateAsync(sessionHash);
        return userId is null ? null : (userId, sessionHash);
    }

    private static bool TryHashBearerToken(string? authorization, TokenService tokens, out string sessionHash)
    {
        sessionHash = string.Empty;
        if (authorization is null) return false;

        var value = authorization.AsSpan().Trim();
        var separatorIndex = value.IndexOf(' ');
        if (separatorIndex <= 0) return false;

        var scheme = value[..separatorIndex];
        var token = value[(separatorIndex + 1)..].TrimStart();
        if (!scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) || !tokens.IsValid(token)) return false;
        sessionHash = tokens.Hash(token);
        return true;
    }
}
