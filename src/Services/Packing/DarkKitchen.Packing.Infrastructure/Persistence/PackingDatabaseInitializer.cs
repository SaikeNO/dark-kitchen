using DarkKitchen.Packing.Features.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarkKitchen.Packing.Infrastructure.Persistence;

public static class PackingDatabaseInitializer
{
    public static async Task InitializePackingDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PackingDbContext>();

        await db.Database.MigrateAsync();
    }
}
