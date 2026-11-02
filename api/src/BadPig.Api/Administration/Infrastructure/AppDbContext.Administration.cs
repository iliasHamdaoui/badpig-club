using BadPig.Administration.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

/// <summary>Part of the application DbContext owned by the Administration module: its DbSets and entity configuration.</summary>
public sealed partial class AppDbContext
{
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<DisabledAccount> DisabledAccounts => Set<DisabledAccount>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<SchoolHoliday> SchoolHolidays => Set<SchoolHoliday>();
    public DbSet<MemberLicence> MemberLicences => Set<MemberLicence>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Title> Titles => Set<Title>();
    public DbSet<BoardTitle> BoardTitles => Set<BoardTitle>();
    public DbSet<AdministrationAuditEntry> AdministrationAuditLog => Set<AdministrationAuditEntry>();

    private static partial void ConfigureAdministration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invitation>(e =>
        {
            e.ToTable("invitations");
            e.HasKey(x => x.TargetId).HasName("invitations_pkey");
            e.Property(x => x.TargetId).HasColumnName("target_id");
            e.Property(x => x.InvitedById).HasColumnName("invited_by_id");
            e.Property(x => x.InvitedByName).HasColumnName("invited_by_name");
            e.Property(x => x.MembershipKind).HasColumnName("membership_kind");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<DisabledAccount>(e =>
        {
            e.ToTable("disabled_accounts");
            e.HasKey(x => x.AccountId).HasName("disabled_accounts_pkey");
            e.Property(x => x.AccountId).HasColumnName("account_id");
            e.Property(x => x.DisabledSince).HasColumnName("disabled_since");
        });

        modelBuilder.Entity<Season>(e =>
        {
            e.ToTable("seasons");
            e.HasKey(x => x.Id).HasName("seasons_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.StartsOn).HasColumnName("starts_on");
            e.Property(x => x.EndsOn).HasColumnName("ends_on");
            e.Property(x => x.GraceDays).HasColumnName("grace_days");
            e.Property(x => x.InternalFee).HasColumnName("internal_fee").HasPrecision(8, 2);
            e.Property(x => x.ExternalFee).HasColumnName("external_fee").HasPrecision(8, 2);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            // The unique index on lower(name) ("seasons_name_uq") is created by the SQL migration scripts.
        });

        modelBuilder.Entity<Membership>(e =>
        {
            e.ToTable("memberships");
            e.HasKey(x => new { x.MemberId, x.SeasonId }).HasName("memberships_pkey");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.SeasonId).HasColumnName("season_id");
            e.Property(x => x.Kind).HasColumnName("kind");
            e.Property(x => x.Amount).HasColumnName("amount").HasPrecision(8, 2);
            e.Property(x => x.ValidatedAt).HasColumnName("validated_at");
            e.Property(x => x.ValidatedById).HasColumnName("validated_by_id");
            e.Property(x => x.ValidatedByName).HasColumnName("validated_by_name");
            e.HasOne(x => x.Season).WithMany().HasForeignKey(x => x.SeasonId)
                .HasConstraintName("memberships_season_id_fkey").OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MemberLicence>(e =>
        {
            e.ToTable("member_licences");
            e.HasKey(x => x.AccountId).HasName("member_licences_pkey");
            e.Property(x => x.AccountId).HasColumnName("account_id");
            e.Property(x => x.Number).HasColumnName("number");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<SchoolHoliday>(e =>
        {
            e.ToTable("school_holidays");
            e.HasKey(x => x.Id).HasName("school_holidays_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.SeasonId).HasColumnName("season_id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.StartsOn).HasColumnName("starts_on");
            e.Property(x => x.EndsOn).HasColumnName("ends_on");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne<Season>().WithMany().HasForeignKey(x => x.SeasonId)
                .HasConstraintName("school_holidays_season_id_fkey").OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.SeasonId, x.StartsOn }).HasDatabaseName("school_holidays_season_id_idx");
        });

        modelBuilder.Entity<Title>(e =>
        {
            e.ToTable("titles", t => t.HasCheckConstraint("titles_label_check", "char_length(label) BETWEEN 1 AND 60"));
            e.HasKey(x => x.Id).HasName("titles_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Label).HasColumnName("label");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            // The unique index on lower(label) ("titles_label_uq") is created by the SQL migration scripts:
            // an expression index cannot be described in the EF model.
        });

        modelBuilder.Entity<BoardTitle>(e =>
        {
            e.ToTable("board_titles");
            e.HasKey(x => x.MemberId).HasName("board_titles_pkey");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.TitleId).HasColumnName("title_id");
            e.HasIndex(x => x.TitleId).IsUnique().HasDatabaseName("board_titles_title_uq");
            e.HasOne(x => x.Title).WithMany().HasForeignKey(x => x.TitleId)
                .HasConstraintName("board_titles_title_id_fkey").OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdministrationAuditEntry>(e =>
        {
            e.ToTable("administration_audit_log");
            e.HasKey(x => x.Id).HasName("administration_audit_log_pkey");
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            e.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            e.Property(x => x.ActorId).HasColumnName("actor_id");
            e.Property(x => x.ActorName).HasColumnName("actor_name");
            e.Property(x => x.Action).HasColumnName("action");
            e.Property(x => x.TargetId).HasColumnName("target_id");
            e.HasIndex(x => x.OccurredAt).IsDescending().HasDatabaseName("administration_audit_log_occurred_at_idx");
        });
    }
}
