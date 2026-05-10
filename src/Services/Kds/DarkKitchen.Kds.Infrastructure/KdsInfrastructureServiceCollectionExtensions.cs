using DarkKitchen.Kds.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Kds.Infrastructure;

public static class KdsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddKdsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextWithWolverineIntegration<KdsDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("kds-db")
                ?? throw new InvalidOperationException("Missing required connection string 'kds-db'.");
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(KdsDbContextFactory).Assembly.FullName));
        });

        return services;
    }
}
