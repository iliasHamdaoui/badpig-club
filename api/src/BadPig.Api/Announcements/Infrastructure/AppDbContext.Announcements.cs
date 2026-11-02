using BadPig.Announcements.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

/// <summary>Part of the application DbContext owned by the Announcements module: its DbSets and entity configuration.</summary>
public sealed partial class AppDbContext
{
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementFile> AnnouncementFiles => Set<AnnouncementFile>();
    public DbSet<AnnouncementLike> AnnouncementLikes => Set<AnnouncementLike>();
    public DbSet<AnnouncementPoll> AnnouncementPolls => Set<AnnouncementPoll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<PollVote> PollVotes => Set<PollVote>();
    public DbSet<AnnouncementComment> AnnouncementComments => Set<AnnouncementComment>();

    private static partial void ConfigureAnnouncements(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(e =>
        {
            e.ToTable("announcements");
            e.HasKey(x => x.Id).HasName("announcements_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Title).HasColumnName("title");
            e.Property(x => x.BodyHtml).HasColumnName("body_html");
            e.Property(x => x.Summary).HasColumnName("summary");
            e.Property(x => x.Kind).HasColumnName("kind");
            e.Property(x => x.Form).HasColumnName("form");
            e.Property(x => x.CommentsEnabled).HasColumnName("comments_enabled");
            e.Property(x => x.IsPublished).HasColumnName("is_published");
            e.Property(x => x.IsPinned).HasColumnName("is_pinned");
            e.Property(x => x.AuthorId).HasColumnName("author_id");
            e.Property(x => x.AuthorName).HasColumnName("author_name");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.PublishedAt).HasColumnName("published_at");
            e.Property(x => x.ExpiresOn).HasColumnName("expires_on");
            e.HasMany(x => x.Files).WithOne().HasForeignKey(f => f.AnnouncementId)
                .HasConstraintName("announcement_files_announcement_id_fkey").OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnnouncementFile>(e =>
        {
            e.ToTable("announcement_files");
            e.HasKey(x => x.Id).HasName("announcement_files_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AnnouncementId).HasColumnName("announcement_id");
            e.Property(x => x.Role).HasColumnName("role");
            e.Property(x => x.FileName).HasColumnName("file_name");
            e.Property(x => x.ContentType).HasColumnName("content_type");
            e.Property(x => x.SizeBytes).HasColumnName("size_bytes");
            e.Property(x => x.StorageKey).HasColumnName("storage_key");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<AnnouncementLike>(e =>
        {
            e.ToTable("announcement_likes");
            e.HasKey(x => new { x.AnnouncementId, x.MemberId }).HasName("announcement_likes_pkey");
            e.Property(x => x.AnnouncementId).HasColumnName("announcement_id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.LikedAt).HasColumnName("liked_at");
        });

        modelBuilder.Entity<AnnouncementComment>(e =>
        {
            e.ToTable("announcement_comments");
            e.HasKey(x => x.Id).HasName("announcement_comments_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AnnouncementId).HasColumnName("announcement_id");
            e.Property(x => x.ParentId).HasColumnName("parent_id");
            e.Property(x => x.AuthorId).HasColumnName("author_id");
            e.Property(x => x.AuthorName).HasColumnName("author_name");
            e.Property(x => x.Text).HasColumnName("text");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<AnnouncementPoll>(e =>
        {
            e.ToTable("announcement_polls");
            e.HasKey(x => x.AnnouncementId).HasName("announcement_polls_pkey");
            e.Property(x => x.AnnouncementId).HasColumnName("announcement_id");
            e.Property(x => x.AllowMultiple).HasColumnName("allow_multiple");
            e.Property(x => x.IsAnonymous).HasColumnName("is_anonymous");
            e.Property(x => x.ClosesOn).HasColumnName("closes_on");
            // Declared so that EF inserts the announcement before its poll when both are created in the same save.
            e.HasOne<Announcement>().WithOne().HasForeignKey<AnnouncementPoll>(x => x.AnnouncementId)
                .HasConstraintName("announcement_polls_announcement_id_fkey").OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Options).WithOne().HasForeignKey(o => o.AnnouncementId)
                .HasConstraintName("announcement_poll_options_announcement_id_fkey").OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PollOption>(e =>
        {
            e.ToTable("announcement_poll_options");
            e.HasKey(x => x.Id).HasName("announcement_poll_options_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AnnouncementId).HasColumnName("announcement_id");
            e.Property(x => x.Label).HasColumnName("label");
            e.Property(x => x.Position).HasColumnName("position");
        });

        modelBuilder.Entity<PollVote>(e =>
        {
            e.ToTable("announcement_poll_votes");
            e.HasKey(x => new { x.OptionId, x.MemberId }).HasName("announcement_poll_votes_pkey");
            e.Property(x => x.OptionId).HasColumnName("option_id");
            e.Property(x => x.AnnouncementId).HasColumnName("announcement_id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.VotedAt).HasColumnName("voted_at");
        });
    }
}
