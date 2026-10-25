namespace Acderator.Server.Services;

internal static class Authentication
{
    public static async Task<(string UserId, string SessionHash)?> AuthenticateAsync(string? authorization, GameRepository repository, TokenService tokens)
    {
        if (authorization is null) return null;
        var parts = authorization.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase) || !tokens.IsValid(parts[1]))
        {
            return null;
        }

        var sessionHash = tokens.Hash(parts[1]);
        var userId = await repository.AuthenticateAsync(sessionHash);
        return userId is null ? null : (userId, sessionHash);
    }
}
