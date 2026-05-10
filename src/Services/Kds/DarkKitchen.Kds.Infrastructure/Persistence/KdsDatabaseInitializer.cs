using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.Infrastructure.Persistence;

public static class KdsDatabaseInitializer
{
    public static async Task InitializeKdsDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KdsDbContext>();

        await db.Database.MigrateAsync();
    }
}
