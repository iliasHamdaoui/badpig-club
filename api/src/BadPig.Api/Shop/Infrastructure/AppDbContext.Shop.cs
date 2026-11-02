using BadPig.Shop.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

/// <summary>Part of the application DbContext owned by the Shop module: its DbSets and entity configuration.</summary>
public sealed partial class AppDbContext
{
    public DbSet<Product> ShopProducts => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ShopAuditEntry> ShopAuditLog => Set<ShopAuditEntry>();

    private static partial void ConfigureShop(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductImage>(e =>
        {
            e.ToTable("shop_product_images");
            e.HasKey(x => x.ProductId).HasName("shop_product_images_pkey");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.ContentType).HasColumnName("content_type");
            e.Property(x => x.Content).HasColumnName("content");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ShopAuditEntry>(e =>
        {
            e.ToTable("shop_audit_log");
            e.HasKey(x => x.Id).HasName("shop_audit_log_pkey");
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            e.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            e.Property(x => x.ActorId).HasColumnName("actor_id");
            e.Property(x => x.ActorName).HasColumnName("actor_name");
            e.Property(x => x.Action).HasColumnName("action");
            e.Property(x => x.TargetId).HasColumnName("target_id");
            e.Property(x => x.Subject).HasColumnName("subject");
            e.Property(x => x.Details).HasColumnName("details");
            e.HasIndex(x => x.OccurredAt).IsDescending().HasDatabaseName("shop_audit_log_occurred_at_idx");
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("shop_products");
            e.HasKey(x => x.Id).HasName("shop_products_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.ProductId)
                .HasConstraintName("shop_product_variants_product_id_fkey").OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.ToTable("shop_product_variants");
            e.HasKey(x => x.Id).HasName("shop_product_variants_pkey");
            e.Ignore(x => x.Available);
            e.Ignore(x => x.IsLowStock);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.Label).HasColumnName("label");
            e.Property(x => x.PriceCents).HasColumnName("price_cents");
            e.Property(x => x.Quantity).HasColumnName("quantity");
            e.Property(x => x.ReservedQuantity).HasColumnName("reserved_quantity");
            e.Property(x => x.LowStockThreshold).HasColumnName("low_stock_threshold");
            e.Property(x => x.LowStockAlerted).HasColumnName("low_stock_alerted");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.ToTable("shop_stock_movements");
            e.HasKey(x => x.Id).HasName("shop_stock_movements_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.VariantId).HasColumnName("variant_id");
            e.Property(x => x.Change).HasColumnName("change");
            e.Property(x => x.Reason).HasColumnName("reason");
            e.Property(x => x.Note).HasColumnName("note");
            e.Property(x => x.OrderId).HasColumnName("order_id");
            e.Property(x => x.AuthorId).HasColumnName("author_id");
            e.Property(x => x.AuthorName).HasColumnName("author_name");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
