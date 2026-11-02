using BadPig.Interclubs.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

/// <summary>Part of the application DbContext owned by the Interclubs module: its DbSets and entity configuration.</summary>
public sealed partial class AppDbContext
{
    public DbSet<Team> InterclubTeams => Set<Team>();
    public DbSet<TeamMember> InterclubTeamMembers => Set<TeamMember>();
    public DbSet<Fixture> InterclubFixtures => Set<Fixture>();
    public DbSet<FixtureMatch> InterclubFixtureMatches => Set<FixtureMatch>();
    public DbSet<DateProposal> InterclubDateProposals => Set<DateProposal>();
    public DbSet<ProposedDate> InterclubProposedDates => Set<ProposedDate>();
    public DbSet<DateAnswer> InterclubDateAnswers => Set<DateAnswer>();

    private static partial void ConfigureInterclubs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(e =>
        {
            e.ToTable("interclub_teams");
            e.HasKey(x => x.Id).HasName("interclub_teams_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.SeasonId).HasColumnName("season_id");
            e.Property(x => x.CaptainId).HasColumnName("captain_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => new { x.SeasonId, x.Name }).IsUnique().HasDatabaseName("interclub_teams_season_id_name_key");
        });

        modelBuilder.Entity<TeamMember>(e =>
        {
            e.ToTable("interclub_team_members");
            e.HasKey(x => new { x.TeamId, x.AccountId }).HasName("interclub_team_members_pkey");
            e.Property(x => x.TeamId).HasColumnName("team_id");
            e.Property(x => x.AccountId).HasColumnName("account_id");
            e.Property(x => x.AddedAt).HasColumnName("added_at");
            e.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId)
                .HasConstraintName("interclub_team_members_team_id_fkey").OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.AccountId).HasDatabaseName("interclub_team_members_account_id_idx");
        });

        modelBuilder.Entity<Fixture>(e =>
        {
            e.ToTable("interclub_fixtures");
            e.HasKey(x => x.Id).HasName("interclub_fixtures_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TeamId).HasColumnName("team_id");
            e.Property(x => x.PlayedOn).HasColumnName("played_on");
            e.Property(x => x.StartTime).HasColumnName("start_time");
            e.Property(x => x.Opponent).HasColumnName("opponent");
            e.Property(x => x.Venue).HasColumnName("venue");
            e.Property(x => x.Place).HasColumnName("place");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId)
                .HasConstraintName("interclub_fixtures_team_id_fkey").OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.TeamId, x.PlayedOn }).HasDatabaseName("interclub_fixtures_team_id_played_on_idx");
        });

        modelBuilder.Entity<FixtureMatch>(e =>
        {
            e.ToTable("interclub_fixture_matches");
            e.HasKey(x => new { x.FixtureId, x.Slot }).HasName("interclub_fixture_matches_pkey");
            e.Ignore(x => x.Players);
            e.Property(x => x.FixtureId).HasColumnName("fixture_id");
            e.Property(x => x.Slot).HasColumnName("slot");
            e.Property(x => x.Player1Id).HasColumnName("player1_id");
            e.Property(x => x.Player2Id).HasColumnName("player2_id");
            e.Ignore(x => x.Sets);
            e.Ignore(x => x.Outcome);
            e.Property(x => x.Ending).HasColumnName("ending");
            e.Property(x => x.Withdrawn).HasColumnName("withdrawn");
            e.Property(x => x.Set1Club).HasColumnName("set1_club");
            e.Property(x => x.Set1Opponent).HasColumnName("set1_opponent");
            e.Property(x => x.Set2Club).HasColumnName("set2_club");
            e.Property(x => x.Set2Opponent).HasColumnName("set2_opponent");
            e.Property(x => x.Set3Club).HasColumnName("set3_club");
            e.Property(x => x.Set3Opponent).HasColumnName("set3_opponent");
            e.HasOne<Fixture>().WithMany().HasForeignKey(x => x.FixtureId)
                .HasConstraintName("interclub_fixture_matches_fixture_id_fkey").OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.Player1Id).HasDatabaseName("interclub_fixture_matches_player1_id_idx");
            e.HasIndex(x => x.Player2Id).HasDatabaseName("interclub_fixture_matches_player2_id_idx");
        });

        modelBuilder.Entity<DateProposal>(e =>
        {
            e.ToTable("interclub_date_proposals");
            e.HasKey(x => x.Id).HasName("interclub_date_proposals_pkey");
            e.Ignore(x => x.IsOpen);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TeamId).HasColumnName("team_id");
            e.Property(x => x.Opponent).HasColumnName("opponent");
            e.Property(x => x.Venue).HasColumnName("venue");
            e.Property(x => x.Place).HasColumnName("place");
            e.Property(x => x.FixtureId).HasColumnName("fixture_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId)
                .HasConstraintName("interclub_date_proposals_team_id_fkey").OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Dates).WithOne().HasForeignKey(d => d.ProposalId)
                .HasConstraintName("interclub_proposed_dates_proposal_id_fkey").OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TeamId).HasDatabaseName("interclub_date_proposals_team_id_idx");
        });

        modelBuilder.Entity<ProposedDate>(e =>
        {
            e.ToTable("interclub_proposed_dates");
            e.HasKey(x => x.Id).HasName("interclub_proposed_dates_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProposalId).HasColumnName("proposal_id");
            e.Property(x => x.PlayedOn).HasColumnName("played_on");
            e.Property(x => x.StartTime).HasColumnName("start_time");
            e.HasMany(x => x.Answers).WithOne().HasForeignKey(a => a.ProposedDateId)
                .HasConstraintName("interclub_date_answers_proposed_date_id_fkey").OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.ProposalId).HasDatabaseName("interclub_proposed_dates_proposal_id_idx");
        });

        modelBuilder.Entity<DateAnswer>(e =>
        {
            e.ToTable("interclub_date_answers");
            e.HasKey(x => new { x.ProposedDateId, x.AccountId }).HasName("interclub_date_answers_pkey");
            e.Property(x => x.ProposedDateId).HasColumnName("proposed_date_id");
            e.Property(x => x.AccountId).HasColumnName("account_id");
            e.Property(x => x.Answer).HasColumnName("answer");
            e.Property(x => x.AnsweredAt).HasColumnName("answered_at");
            e.HasIndex(x => x.AccountId).HasDatabaseName("interclub_date_answers_account_id_idx");
        });
    }
}
