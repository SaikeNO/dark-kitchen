using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DarkKitchen.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__inventory-db")
            ?? "Host=localhost;Port=5432;Database=darkkitchen_inventory;Username=postgres;Password=postgres";

        options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(InventoryDbContextFactory).Assembly.FullName));

        return new InventoryDbContext(options.Options);
    }
}
