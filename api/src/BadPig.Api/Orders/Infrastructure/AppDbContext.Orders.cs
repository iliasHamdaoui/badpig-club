using BadPig.Orders.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

/// <summary>Part of the application DbContext owned by the Orders module: its DbSets and entity configuration.</summary>
public sealed partial class AppDbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    private static partial void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(x => x.Id).HasName("orders_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.BuyerId).HasColumnName("buyer_id");
            e.Property(x => x.BuyerName).HasColumnName("buyer_name");
            e.Property(x => x.Status).HasColumnName("status");
            e.Property(x => x.TotalCents).HasColumnName("total_cents");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.PaidAt).HasColumnName("paid_at");
            e.Property(x => x.DeliveredAt).HasColumnName("delivered_at");
            e.Property(x => x.DeliveredById).HasColumnName("delivered_by_id");
            e.Property(x => x.DeliveredByName).HasColumnName("delivered_by_name");
            e.Property(x => x.PaymentSessionId).HasColumnName("payment_session_id");
            e.Property(x => x.CheckoutUrl).HasColumnName("checkout_url");
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.OrderId)
                .HasConstraintName("order_lines_order_id_fkey").OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.ToTable("order_lines");
            e.HasKey(x => x.Id).HasName("order_lines_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.OrderId).HasColumnName("order_id");
            e.Property(x => x.VariantId).HasColumnName("variant_id");
            e.Property(x => x.ProductName).HasColumnName("product_name");
            e.Property(x => x.VariantLabel).HasColumnName("variant_label");
            e.Property(x => x.UnitPriceCents).HasColumnName("unit_price_cents");
            e.Property(x => x.Quantity).HasColumnName("quantity");
        });
    }
}
