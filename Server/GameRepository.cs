using Dapper;
using Acderator.Shared;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Acderator.Server;

public sealed class GameRepository
{
    private readonly MySqlDataSource dataSource;
    private readonly ServerOptions options;

    public GameRepository(MySqlDataSource dataSource, IOptions<ServerOptions> options)
    {
        this.dataSource = dataSource;
        this.options = options.Value;
    }

    internal async Task<UserRecord?> FindUserAsync(string userId)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<UserRecord>(
            "SELECT userid AS UserId, password AS Password FROM user WHERE userid = @UserId LIMIT 1",
            new { UserId = userId });
    }

    internal async Task<bool> UuidExistsAsync(string uuid)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<int>("SELECT EXISTS(SELECT 1 FROM user WHERE uuid = @Uuid)", new { Uuid = uuid }) == 1;
    }

    internal async Task<bool> CreateUserAndSessionAsync(int userId, string uuid, string passwordHash, string sessionHash)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            await connection.ExecuteAsync(
                "INSERT INTO user (userid, uuid, password) VALUES (@UserId, @Uuid, @Password)",
                new { UserId = userId.ToString(), Uuid = uuid, Password = passwordHash }, transaction);
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            return false;
        }
        await PruneAuthSessions(connection, transaction);
        await connection.ExecuteAsync(
            "INSERT INTO session (session_id, userid, last_activity) VALUES (@SessionId, @UserId, UTC_TIMESTAMP())",
            new { SessionId = sessionHash, UserId = userId.ToString() }, transaction);
        await transaction.CommitAsync();
        return true;
    }

    internal async Task CreateLoginSessionAsync(string userId, string sessionHash, string? replacementPasswordHash)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        if (replacementPasswordHash is not null)
        {
            await connection.ExecuteAsync("UPDATE user SET password = @Password WHERE userid = @UserId", new { Password = replacementPasswordHash, UserId = userId }, transaction);
        }
        await PruneAuthSessions(connection, transaction);
        await connection.ExecuteAsync(
            "INSERT INTO session (session_id, userid, last_activity) VALUES (@SessionId, @UserId, UTC_TIMESTAMP())",
            new { SessionId = sessionHash, UserId = userId }, transaction);
        await transaction.CommitAsync();
    }

    internal async Task<string?> AuthenticateAsync(string sessionHash)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        if (await connection.ExecuteAsync(
            "UPDATE session SET last_activity = UTC_TIMESTAMP() WHERE session_id = @SessionId AND last_activity >= @Cutoff",
            new { SessionId = sessionHash, Cutoff = DateTime.UtcNow.AddSeconds(-options.AuthSessionTtlSeconds) }, transaction) != 1)
        {
            return null;
        }
        var userId = await connection.QuerySingleOrDefaultAsync<string>(
            "SELECT userid FROM session WHERE session_id = @SessionId LIMIT 1",
            new { SessionId = sessionHash }, transaction);
        await transaction.CommitAsync();
        return userId;
    }

    internal async Task LogoutAsync(string sessionHash)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync("DELETE FROM session WHERE session_id = @SessionId", new { SessionId = sessionHash });
    }

    internal async Task<List<ScoreEntry>> GetScoresAsync(string userId)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        return (await connection.QueryAsync<ScoreEntry>(
            "SELECT score_id AS ScoreId, score AS Score FROM score WHERE userid = @UserId ORDER BY score_id",
            new { UserId = userId })).AsList();
    }

    internal async Task<bool> BeginScoreAsync(string userId, int scoreId, string sessionHash)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        if (await connection.ExecuteScalarAsync<int>("SELECT EXISTS(SELECT 1 FROM song_master WHERE sid = @ScoreId)", new { ScoreId = scoreId }) != 1)
        {
            return false;
        }

        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync("DELETE FROM score_session WHERE created_at < @Cutoff", new { Cutoff = DateTime.UtcNow.AddSeconds(-options.ScoreSessionTtlSeconds) }, transaction);
        await connection.ExecuteAsync("DELETE FROM score_session WHERE userid = @UserId AND score_id = @ScoreId", new { UserId = userId, ScoreId = scoreId }, transaction);
        await connection.ExecuteAsync(
            "INSERT INTO score_session (session_id, score_id, userid, created_at) VALUES (@SessionId, @ScoreId, @UserId, UTC_TIMESTAMP())",
            new { SessionId = sessionHash, ScoreId = scoreId, UserId = userId }, transaction);
        await transaction.CommitAsync();
        return true;
    }

    internal async Task<SubmitScoreResult> SubmitScoreAsync(string userId, string sessionHash, int score)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        var baseScore = await connection.QuerySingleOrDefaultAsync<int?>("SELECT score FROM song_base_score_master LIMIT 1");
        if (baseScore is null) return SubmitScoreResult.MissingBaseScore;
        if (score < 0 || score > baseScore) return SubmitScoreResult.InvalidScore;

        await using var transaction = await connection.BeginTransactionAsync();
        var scoreId = await connection.QuerySingleOrDefaultAsync<int?>(
            "SELECT score_id FROM score_session WHERE session_id = @SessionId AND userid = @UserId AND created_at >= @Cutoff LIMIT 1 FOR UPDATE",
            new { SessionId = sessionHash, UserId = userId, Cutoff = DateTime.UtcNow.AddSeconds(-options.ScoreSessionTtlSeconds) }, transaction);
        if (scoreId is null) return SubmitScoreResult.SessionNotFound;

        await connection.ExecuteAsync("DELETE FROM score_session WHERE session_id = @SessionId", new { SessionId = sessionHash }, transaction);
        await connection.ExecuteAsync(
            "INSERT INTO score (userid, score_id, score) VALUES (@UserId, @ScoreId, @Score) ON DUPLICATE KEY UPDATE score = GREATEST(score, VALUES(score))",
            new { UserId = userId, ScoreId = scoreId.Value, Score = score }, transaction);
        await transaction.CommitAsync();
        return SubmitScoreResult.Success;
    }

    private async Task PruneAuthSessions(MySqlConnection connection, MySqlTransaction transaction) =>
        await connection.ExecuteAsync("DELETE FROM session WHERE last_activity < @Cutoff", new { Cutoff = DateTime.UtcNow.AddSeconds(-options.AuthSessionTtlSeconds) }, transaction);
}

internal sealed record UserRecord(string UserId, string Password);

internal enum SubmitScoreResult
{
    Success,
    InvalidScore,
    SessionNotFound,
    MissingBaseScore,
}
