using Microsoft.EntityFrameworkCore;

namespace Acderator.Server;

public sealed class AcderatorDbContext(DbContextOptions<AcderatorDbContext> options) : DbContext(options)
{
    internal DbSet<UserEntity> Users => Set<UserEntity>();
    internal DbSet<AuthSessionEntity> AuthSessions => Set<AuthSessionEntity>();
    internal DbSet<ScoreSessionEntity> ScoreSessions => Set<ScoreSessionEntity>();
    internal DbSet<ScoreEntity> Scores => Set<ScoreEntity>();
    internal DbSet<MasterVersionEntity> MasterVersions => Set<MasterVersionEntity>();
    internal DbSet<TitleMasterEntity> TitleMasters => Set<TitleMasterEntity>();
    internal DbSet<SongSelectMasterEntity> SongSelectMasters => Set<SongSelectMasterEntity>();
    internal DbSet<SongMasterEntity> SongMasters => Set<SongMasterEntity>();
    internal DbSet<SongScoreRateMasterEntity> SongScoreRateMasters => Set<SongScoreRateMasterEntity>();
    internal DbSet<SongBaseScoreMasterEntity> SongBaseScoreMasters => Set<SongBaseScoreMasterEntity>();
    internal DbSet<SongJudgeZoneMasterEntity> SongJudgeZoneMasters => Set<SongJudgeZoneMasterEntity>();
    internal DbSet<SongBaseHpMasterEntity> SongBaseHpMasters => Set<SongBaseHpMasterEntity>();
    internal DbSet<SongHpRateMasterEntity> SongHpRateMasters => Set<SongHpRateMasterEntity>();
    internal DbSet<SoundSheetNameMasterEntity> SoundSheetNameMasters => Set<SoundSheetNameMasterEntity>();
    internal DbSet<ResultMasterEntity> ResultMasters => Set<ResultMasterEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).HasColumnName("userid").HasMaxLength(255);
            entity.Property(x => x.Uuid).HasColumnName("uuid").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Password).HasColumnName("password").HasMaxLength(255).IsRequired();
            entity.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("uk_user_uuid");
        });

        modelBuilder.Entity<AuthSessionEntity>(entity =>
        {
            entity.ToTable("session");
            entity.HasKey(x => x.SessionId);
            entity.Property(x => x.SessionId).HasColumnName("session_id").HasMaxLength(255);
            entity.Property(x => x.UserId).HasColumnName("userid").HasMaxLength(255).IsRequired();
            entity.Property(x => x.LastActivity).HasColumnName("last_activity").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.LastActivity).HasDatabaseName("idx_session_last_activity");
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ScoreSessionEntity>(entity =>
        {
            entity.ToTable("score_session");
            entity.HasKey(x => x.SessionId);
            entity.Property(x => x.SessionId).HasColumnName("session_id").HasMaxLength(255);
            entity.Property(x => x.ScoreId).HasColumnName("score_id");
            entity.Property(x => x.UserId).HasColumnName("userid").HasMaxLength(255).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_score_session_created_at");
            entity.HasIndex(x => new { x.UserId, x.ScoreId }).HasDatabaseName("idx_score_session_user_score");
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ScoreEntity>(entity =>
        {
            entity.ToTable("score");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("userid").HasMaxLength(255).IsRequired();
            entity.Property(x => x.ScoreId).HasColumnName("score_id");
            entity.Property(x => x.Value).HasColumnName("score");
            entity.Property(x => x.SubmittedAt).HasColumnName("submitted_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
            entity.HasIndex(x => new { x.UserId, x.ScoreId }).IsUnique().HasDatabaseName("uk_userid_score_id");
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MasterVersionEntity>(entity =>
        {
            entity.ToTable("master_version");
            entity.HasKey(x => x.Version);
            entity.Property(x => x.Version).HasColumnName("version").HasMaxLength(255);
        });

        modelBuilder.Entity<TitleMasterEntity>(entity =>
        {
            entity.ToTable("title_master");
            entity.HasKey(x => x.Tid);
            entity.Property(x => x.Tid).HasColumnName("tid").ValueGeneratedNever();
        });

        modelBuilder.Entity<SongSelectMasterEntity>(entity =>
        {
            entity.ToTable("song_select_master");
            entity.HasKey(x => x.Group);
            entity.Property(x => x.Group).HasColumnName("group").ValueGeneratedNever();
            entity.Property(x => x.StartSongTime).HasColumnName("start_song_time");
            entity.Property(x => x.SongTime).HasColumnName("song_time");
        });

        modelBuilder.Entity<SongMasterEntity>(entity =>
        {
            entity.ToTable("song_master");
            entity.HasKey(x => x.Sid);
            entity.Property(x => x.Sid).HasColumnName("sid").ValueGeneratedNever();
            entity.Property(x => x.Group).HasColumnName("group");
            entity.Property(x => x.Difficulty).HasColumnName("difficulty");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Composer).HasColumnName("composer").HasMaxLength(255).IsRequired();
            entity.Property(x => x.StartOffset).HasColumnName("start_offset");
            entity.Property(x => x.Bg).HasColumnName("bg");
        });

        modelBuilder.Entity<SongScoreRateMasterEntity>(entity =>
        {
            entity.ToTable("song_score_rate_master");
            entity.HasKey(x => x.RType);
            entity.Property(x => x.RType).HasColumnName("r_type").ValueGeneratedNever();
            entity.Property(x => x.Rate).HasColumnName("rate");
        });

        modelBuilder.Entity<SongBaseScoreMasterEntity>(entity =>
        {
            entity.ToTable("song_base_score_master");
            entity.HasKey(x => x.Score);
            entity.Property(x => x.Score).HasColumnName("score").ValueGeneratedNever();
        });

        modelBuilder.Entity<SongJudgeZoneMasterEntity>(entity =>
        {
            entity.ToTable("song_judge_zone_master");
            entity.HasKey(x => x.JType);
            entity.Property(x => x.JType).HasColumnName("j_type").ValueGeneratedNever();
            entity.Property(x => x.Zone).HasColumnName("zone");
        });

        modelBuilder.Entity<SongBaseHpMasterEntity>(entity =>
        {
            entity.ToTable("song_base_hp_master");
            entity.HasKey(x => x.Hp);
            entity.Property(x => x.Hp).HasColumnName("hp").ValueGeneratedNever();
        });

        modelBuilder.Entity<SongHpRateMasterEntity>(entity =>
        {
            entity.ToTable("song_hp_rate_master");
            entity.HasKey(x => x.JType);
            entity.Property(x => x.JType).HasColumnName("j_type").ValueGeneratedNever();
            entity.Property(x => x.Rate).HasColumnName("rate");
        });

        modelBuilder.Entity<SoundSheetNameMasterEntity>(entity =>
        {
            entity.ToTable("sound_sheet_name_master");
            entity.HasKey(x => new { x.Category, x.Id });
            entity.Property(x => x.Category).HasColumnName("category");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SheetName).HasColumnName("sheet_name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.CueName).HasColumnName("cue_name").HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<ResultMasterEntity>(entity =>
        {
            entity.ToTable("result_master");
            entity.HasKey(x => x.Rid);
            entity.Property(x => x.Rid).HasColumnName("rid").ValueGeneratedNever();
        });
    }
}

internal sealed class UserEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

internal sealed class AuthSessionEntity
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
}

internal sealed class ScoreSessionEntity
{
    public string SessionId { get; set; } = string.Empty;
    public int ScoreId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

internal sealed class ScoreEntity
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ScoreId { get; set; }
    public int Value { get; set; }
    public DateTime SubmittedAt { get; set; }
}

internal sealed class MasterVersionEntity
{
    public string Version { get; set; } = string.Empty;
}

internal sealed class TitleMasterEntity
{
    public int Tid { get; set; }
}

internal sealed class SongSelectMasterEntity
{
    public int Group { get; set; }
    public int StartSongTime { get; set; }
    public int SongTime { get; set; }
}

internal sealed class SongMasterEntity
{
    public int Sid { get; set; }
    public int Group { get; set; }
    public int Difficulty { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Composer { get; set; } = string.Empty;
    public float StartOffset { get; set; }
    public int Bg { get; set; }
}

internal sealed class SongScoreRateMasterEntity
{
    public int RType { get; set; }
    public float Rate { get; set; }
}

internal sealed class SongBaseScoreMasterEntity
{
    public int Score { get; set; }
}

internal sealed class SongJudgeZoneMasterEntity
{
    public int JType { get; set; }
    public float Zone { get; set; }
}

internal sealed class SongBaseHpMasterEntity
{
    public int Hp { get; set; }
}

internal sealed class SongHpRateMasterEntity
{
    public int JType { get; set; }
    public int Rate { get; set; }
}

internal sealed class SoundSheetNameMasterEntity
{
    public int Category { get; set; }
    public int Id { get; set; }
    public string SheetName { get; set; } = string.Empty;
    public string CueName { get; set; } = string.Empty;
}

internal sealed class ResultMasterEntity
{
    public int Rid { get; set; }
}
