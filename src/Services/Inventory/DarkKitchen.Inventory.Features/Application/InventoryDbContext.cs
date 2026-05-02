using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Application;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<WarehouseItem> WarehouseItems => Set<WarehouseItem>();
    public DbSet<RecipeSnapshot> RecipeSnapshots => Set<RecipeSnapshot>();
    public DbSet<RecipeSnapshotItem> RecipeSnapshotItems => Set<RecipeSnapshotItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<StockReservationLine> StockReservationLines => Set<StockReservationLine>();
    public DbSet<InventoryLog> InventoryLogs => Set<InventoryLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.MapWolverineEnvelopeStorage();
        ConfigureInventory(builder);
    }

    private static void ConfigureInventory(ModelBuilder builder)
    {
        builder.Entity<WarehouseItem>(entity =>
        {
            entity.ToTable("warehouse_items", "inventory");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(160).IsRequired();
            entity.Property(item => item.Unit).HasMaxLength(32).IsRequired();
            entity.Property(item => item.OnHandQuantity).HasPrecision(12, 3);
            entity.Property(item => item.ReservedQuantity).HasPrecision(12, 3);
            entity.Property(item => item.MinSafetyLevel).HasPrecision(12, 3);
            entity.Ignore(item => item.AvailableQuantity);
            entity.Ignore(item => item.IsBelowSafetyLevel);
            entity.Ignore(item => item.ReorderQuantity);
            entity.HasIndex(item => item.Name);
        });

        builder.Entity<RecipeSnapshot>(entity =>
        {
            entity.ToTable("recipe_snapshots", "inventory");
            entity.HasKey(recipe => recipe.ProductId);
            entity.HasMany(recipe => recipe.Items)
                .WithOne(item => item.Recipe)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RecipeSnapshotItem>(entity =>
        {
            entity.ToTable("recipe_snapshot_items", "inventory");
            entity.HasKey(item => new { item.ProductId, item.IngredientId });
            entity.Property(item => item.IngredientName).HasMaxLength(160).IsRequired();
            entity.Property(item => item.Unit).HasMaxLength(32).IsRequired();
            entity.Property(item => item.Quantity).HasPrecision(12, 3);
        });

        builder.Entity<StockReservation>(entity =>
        {
            entity.ToTable("stock_reservations", "inventory");
            entity.HasKey(reservation => reservation.Id);
            entity.Property(reservation => reservation.FailureReasonCode).HasMaxLength(64);
            entity.HasIndex(reservation => reservation.OrderId).IsUnique();
            entity.HasMany(reservation => reservation.Lines)
                .WithOne(line => line.Reservation)
                .HasForeignKey(line => line.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockReservationLine>(entity =>
        {
            entity.ToTable("stock_reservation_lines", "inventory");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.Quantity).HasPrecision(12, 3);
            entity.HasOne(line => line.WarehouseItem)
                .WithMany()
                .HasForeignKey(line => line.WarehouseItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(line => new { line.ReservationId, line.WarehouseItemId }).IsUnique();
        });

        builder.Entity<InventoryLog>(entity =>
        {
            entity.ToTable("inventory_logs", "inventory");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Amount).HasPrecision(12, 3);
            entity.Property(log => log.OnHandAfter).HasPrecision(12, 3);
            entity.Property(log => log.ReservedAfter).HasPrecision(12, 3);
            entity.Property(log => log.Note).HasMaxLength(500);
            entity.HasOne(log => log.WarehouseItem)
                .WithMany()
                .HasForeignKey(log => log.WarehouseItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(log => log.WarehouseItemId);
            entity.HasIndex(log => log.OrderId);
            entity.HasIndex(log => log.CreatedAt);
        });
    }
}
