using Acderator.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Data;

namespace Acderator.Server;

public sealed class GameRepository(IDbContextFactory<AcderatorDbContext> contextFactory, IOptions<ServerOptions> options)
{
    private readonly IDbContextFactory<AcderatorDbContext> contextFactory = contextFactory;
    private readonly ServerOptions options = options.Value;

    internal async Task<UserRecord?> FindUserAsync(string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.AsNoTracking().Where(x => x.UserId == userId).Select(x => new UserRecord(x.UserId, x.Password)).SingleOrDefaultAsync();
    }

    internal async Task<bool> UuidExistsAsync(string uuid)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.AnyAsync(x => x.Uuid == uuid);
    }

    internal async Task<bool> CreateUserAndSessionAsync(int userId, string uuid, string passwordHash, string sessionHash)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var userIdText = userId.ToString();
        var now = DateTime.UtcNow;
        context.Users.Add(new UserEntity { UserId = userIdText, Uuid = uuid, Password = passwordHash });
        AddAuthSession(context, userIdText, sessionHash, now);
        await PruneAuthSessionsAsync(context);
        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (DbUpdateException exception) when (IsDuplicateKey(exception))
        {
            return false;
        }
    }

    internal async Task CreateLoginSessionAsync(string userId, string sessionHash, string? replacementPasswordHash)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();
        if (replacementPasswordHash is not null)
        {
            var user = await context.Users.SingleAsync(x => x.UserId == userId);
            user.Password = replacementPasswordHash;
        }
        await PruneAuthSessionsAsync(context);
        AddAuthSession(context, userId, sessionHash, DateTime.UtcNow);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    internal async Task<string?> AuthenticateAsync(string sessionHash)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var now = DateTime.UtcNow;
        var cutoff = now.AddSeconds(-options.AuthSessionTtlSeconds);
        var activeSession = context.AuthSessions.Where(x => x.SessionId == sessionHash && x.LastActivity >= cutoff);
        var session = await activeSession.SingleOrDefaultAsync();
        if (session is null) return null;
        session.LastActivity = now;
        await context.SaveChangesAsync();
        return session.UserId;
    }

    internal async Task LogoutAsync(string sessionHash)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.AuthSessions.Where(x => x.SessionId == sessionHash).ExecuteDeleteAsync();
    }

    internal async Task<List<ScoreEntry>> GetScoresAsync(string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var scores = context.Scores.AsNoTracking().Where(x => x.UserId == userId).OrderBy(x => x.ScoreId);
        return await scores.Select(x => new ScoreEntry { ScoreId = x.ScoreId, Score = x.Value }).ToListAsync();
    }

    internal async Task<bool> BeginScoreAsync(string userId, int scoreId, string sessionHash)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        if (!await context.SongMasters.AnyAsync(x => x.Sid == scoreId)) return false;

        await using var transaction = await context.Database.BeginTransactionAsync();
        var now = DateTime.UtcNow;
        var cutoff = now.AddSeconds(-options.ScoreSessionTtlSeconds);
        await context.ScoreSessions.Where(x => x.CreatedAt < cutoff).ExecuteDeleteAsync();
        await context.ScoreSessions.Where(x => x.UserId == userId && x.ScoreId == scoreId).ExecuteDeleteAsync();
        context.ScoreSessions.Add(new ScoreSessionEntity
        {
            SessionId = sessionHash,
            ScoreId = scoreId,
            UserId = userId,
            CreatedAt = now,
        });
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    internal async Task<SubmitScoreResult> SubmitScoreAsync(string userId, string sessionHash, int score)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var baseScore = await context.SongBaseScoreMasters.AsNoTracking().Select(x => (int?)x.Score).FirstOrDefaultAsync();
        if (baseScore is null) return SubmitScoreResult.MissingBaseScore;
        if (score < 0 || score > baseScore) return SubmitScoreResult.InvalidScore;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var cutoff = DateTime.UtcNow.AddSeconds(-options.ScoreSessionTtlSeconds);
        var activeScoreSession = context.ScoreSessions.Where(x => x.SessionId == sessionHash && x.UserId == userId && x.CreatedAt >= cutoff);
        var scoreSession = await activeScoreSession.SingleOrDefaultAsync();
        if (scoreSession is null) return SubmitScoreResult.SessionNotFound;

        context.ScoreSessions.Remove(scoreSession);
        var savedScores = context.Scores.Where(x => x.UserId == userId && x.ScoreId == scoreSession.ScoreId);
        var savedScore = await savedScores.SingleOrDefaultAsync();
        if (savedScore is null)
        {
            context.Scores.Add(new ScoreEntity { UserId = userId, ScoreId = scoreSession.ScoreId, Value = score });
        }
        else if (score > savedScore.Value)
        {
            savedScore.Value = score;
        }
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return SubmitScoreResult.Success;
    }

    private async Task PruneAuthSessionsAsync(AcderatorDbContext context)
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-options.AuthSessionTtlSeconds);
        await context.AuthSessions.Where(x => x.LastActivity < cutoff).ExecuteDeleteAsync();
    }

    private void AddAuthSession(AcderatorDbContext context, string userId, string sessionHash, DateTime now)
    {
        context.AuthSessions.Add(new AuthSessionEntity { SessionId = sessionHash, UserId = userId, LastActivity = now });
    }

    private bool IsDuplicateKey(DbUpdateException exception) => exception.GetBaseException() is MySqlException { Number: 1062 };
}

internal sealed record UserRecord(string UserId, string Password);

internal enum SubmitScoreResult
{
    Success,
    InvalidScore,
    SessionNotFound,
    MissingBaseScore,
}
