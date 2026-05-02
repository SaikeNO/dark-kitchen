using DarkKitchen.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Infrastructure;

public static class InventoryInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextWithWolverineIntegration<InventoryDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("inventory-db")
                ?? throw new InvalidOperationException("Missing required connection string 'inventory-db'.");
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(InventoryDbContextFactory).Assembly.FullName));
        });

        services.AddScoped<IInventoryOutbox, WolverineInventoryOutbox>();
        services.AddScoped<InventoryReservationService>();

        return services;
    }
}
