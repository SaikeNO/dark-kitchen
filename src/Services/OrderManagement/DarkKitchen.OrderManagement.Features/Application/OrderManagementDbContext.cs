using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Application;

public sealed class OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderHistory> OrderHistories => Set<OrderHistory>();
    public DbSet<CustomerSnapshot> CustomerSnapshots => Set<CustomerSnapshot>();
    public DbSet<MenuItemSnapshot> MenuItemSnapshots => Set<MenuItemSnapshot>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.MapWolverineEnvelopeStorage();
        ConfigureOrders(builder);
    }

    private static void ConfigureOrders(ModelBuilder builder)
    {
        builder.Entity<Order>(entity =>
        {
            entity.ToTable("orders", "orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.ExternalOrderId).HasMaxLength(120).IsRequired();
            entity.Property(order => order.SourceChannel).HasMaxLength(80).IsRequired();
            entity.Property(order => order.TotalPrice).HasPrecision(10, 2);
            entity.Property(order => order.Currency).HasMaxLength(3).IsRequired();
            entity.HasOne(order => order.Customer)
                .WithOne(customer => customer.Order)
                .HasForeignKey<CustomerSnapshot>(customer => customer.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(order => order.Items)
                .WithOne(item => item.Order)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(order => order.History)
                .WithOne(history => history.Order)
                .HasForeignKey(history => history.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(order => new { order.BrandId, order.SourceChannel, order.ExternalOrderId }).IsUnique();
            entity.HasIndex(order => order.CorrelationId);
            entity.HasIndex(order => order.Status);
        });

        builder.Entity<CustomerSnapshot>(entity =>
        {
            entity.ToTable("customer_snapshots", "orders");
            entity.HasKey(customer => customer.OrderId);
            entity.Property(customer => customer.DisplayName).HasMaxLength(160);
            entity.Property(customer => customer.Phone).HasMaxLength(64);
            entity.Property(customer => customer.DeliveryNote).HasMaxLength(500);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items", "orders");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(160).IsRequired();
            entity.Property(item => item.UnitPrice).HasPrecision(10, 2);
            entity.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            entity.Ignore(item => item.LineTotal);
            entity.HasIndex(item => item.OrderId);
            entity.HasIndex(item => item.MenuItemId);
        });

        builder.Entity<OrderHistory>(entity =>
        {
            entity.ToTable("order_history", "orders");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Reason).HasMaxLength(500);
            entity.HasIndex(history => history.OrderId);
            entity.HasIndex(history => history.CorrelationId);
            entity.HasIndex(history => history.CreatedAt);
        });

        builder.Entity<MenuItemSnapshot>(entity =>
        {
            entity.ToTable("menu_item_snapshots", "orders");
            entity.HasKey(item => item.MenuItemId);
            entity.Property(item => item.Name).HasMaxLength(160).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(1000);
            entity.Property(item => item.Price).HasPrecision(10, 2);
            entity.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            entity.HasIndex(item => item.BrandId);
            entity.HasIndex(item => new { item.BrandId, item.IsActive });
        });
    }
}
