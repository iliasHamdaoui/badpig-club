using BadPig.Usage.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

/// <summary>Part of the application DbContext owned by the Usage module: its DbSets and entity configuration.</summary>
public sealed partial class AppDbContext
{
    public DbSet<PageUsage> PageUsages => Set<PageUsage>();

    private static partial void ConfigureUsage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PageUsage>(e =>
        {
            e.ToTable("page_usage");
            e.HasKey(x => new { x.Day, x.Path }).HasName("page_usage_pkey");
            e.Property(x => x.Day).HasColumnName("day");
            e.Property(x => x.Path).HasColumnName("path");
            e.Property(x => x.Views).HasColumnName("views");
            e.Property(x => x.Seconds).HasColumnName("seconds");
        });
    }
}
