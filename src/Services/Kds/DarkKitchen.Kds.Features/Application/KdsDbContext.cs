using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Kds.Features.Application;

public sealed class KdsDbContext(DbContextOptions<KdsDbContext> options) : DbContext(options)
{
    public DbSet<KitchenTicket> KitchenTickets => Set<KitchenTicket>();
    public DbSet<KitchenTask> KitchenTasks => Set<KitchenTask>();
    public DbSet<KitchenStation> KitchenStations => Set<KitchenStation>();
    public DbSet<ProductStationRouteSnapshot> ProductStationRoutes => Set<ProductStationRouteSnapshot>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.MapWolverineEnvelopeStorage();
        ConfigureKds(builder);
    }

    private static void ConfigureKds(ModelBuilder builder)
    {
        builder.Entity<KitchenTicket>(entity =>
        {
            entity.ToTable("kitchen_tickets", "kds");
            entity.HasKey(ticket => ticket.Id);
            entity.Property(ticket => ticket.SourceChannel).HasMaxLength(64).IsRequired();
            entity.Property(ticket => ticket.Status).HasConversion<int>();
            entity.HasIndex(ticket => ticket.OrderId).IsUnique();
            entity.HasIndex(ticket => ticket.BrandId);
            entity.HasMany(ticket => ticket.Tasks)
                .WithOne(task => task.Ticket)
                .HasForeignKey(task => task.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(ticket => ticket.Tasks)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<KitchenTask>(entity =>
        {
            entity.ToTable("kitchen_tasks", "kds");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.ItemName).HasMaxLength(200).IsRequired();
            entity.Property(task => task.StationCode).HasMaxLength(64);
            entity.Property(task => task.Status).HasConversion<int>();
            entity.HasIndex(task => task.OrderItemId).IsUnique();
            entity.HasIndex(task => new { task.StationId, task.Status, task.CreatedAt });
            entity.HasIndex(task => task.TicketId);
        });

        builder.Entity<KitchenStation>(entity =>
        {
            entity.ToTable("kitchen_stations", "kds");
            entity.HasKey(station => station.Id);
            entity.Property(station => station.Code).HasMaxLength(64).IsRequired();
            entity.Property(station => station.Name).HasMaxLength(160).IsRequired();
            entity.Property(station => station.DisplayColor).HasMaxLength(32).IsRequired();
            entity.HasIndex(station => station.Code).IsUnique();
            entity.HasIndex(station => station.IsActive);
        });

        builder.Entity<ProductStationRouteSnapshot>(entity =>
        {
            entity.ToTable("product_station_route_snapshots", "kds");
            entity.HasKey(route => new { route.BrandId, route.ProductId });
            entity.Property(route => route.StationCode).HasMaxLength(64).IsRequired();
            entity.HasIndex(route => route.StationId);
        });
    }
}
