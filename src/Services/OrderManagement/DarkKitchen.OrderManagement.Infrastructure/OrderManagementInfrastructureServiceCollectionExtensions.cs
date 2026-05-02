using DarkKitchen.OrderManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Infrastructure;

public static class OrderManagementInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOrderManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextWithWolverineIntegration<OrderManagementDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("order-db")
                ?? throw new InvalidOperationException("Missing required connection string 'order-db'.");
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(OrderManagementDbContextFactory).Assembly.FullName));
        });

        return services;
    }
}
