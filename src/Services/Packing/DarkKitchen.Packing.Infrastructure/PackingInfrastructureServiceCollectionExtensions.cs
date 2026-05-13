using DarkKitchen.Packing.Features.Application;
using DarkKitchen.Packing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Packing.Infrastructure;

public static class PackingInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPackingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextWithWolverineIntegration<PackingDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("packing-db")
                ?? throw new InvalidOperationException("Missing required connection string 'packing-db'.");
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(PackingDbContextFactory).Assembly.FullName));
        });

        return services;
    }
}
