using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

/// <summary>
/// The single DbContext of the application (one PostgreSQL database per environment). It is split in partial classes:
/// each module owns a file <c>AppDbContext.&lt;Module&gt;.cs</c> in its own Infrastructure folder, with the DbSets
/// and the entity configuration of its tables. A module only uses its own DbSets; data of another module is
/// reached through that module's Core interfaces.
/// </summary>
public sealed partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureAdministration(modelBuilder);
        ConfigureAnnouncements(modelBuilder);
        ConfigureShop(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureInterclubs(modelBuilder);
        ConfigureProfile(modelBuilder);
    }

    // Implemented by each module (compile error if a module forgets its part).
    private static partial void ConfigureAdministration(ModelBuilder modelBuilder);
    private static partial void ConfigureAnnouncements(ModelBuilder modelBuilder);
    private static partial void ConfigureShop(ModelBuilder modelBuilder);
    private static partial void ConfigureOrders(ModelBuilder modelBuilder);
    private static partial void ConfigureInterclubs(ModelBuilder modelBuilder);
    private static partial void ConfigureProfile(ModelBuilder modelBuilder);
}
