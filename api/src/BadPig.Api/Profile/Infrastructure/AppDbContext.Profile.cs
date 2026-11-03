using BadPig.Profile.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

/// <summary>Part of the application DbContext owned by the Profile module: its DbSets and entity configuration.</summary>
public sealed partial class AppDbContext
{
    public DbSet<MemberPhoto> MemberPhotos => Set<MemberPhoto>();

    private static partial void ConfigureProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemberPhoto>(e =>
        {
            e.ToTable("member_photos");
            e.HasKey(x => x.AccountId).HasName("member_photos_pkey");
            e.Property(x => x.AccountId).HasColumnName("account_id");
            e.Property(x => x.ContentType).HasColumnName("content_type");
            e.Property(x => x.Content).HasColumnName("content");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
