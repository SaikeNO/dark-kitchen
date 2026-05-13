using DarkKitchen.Kds.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DarkKitchen.Kds.Infrastructure.Persistence;

public static class KdsDatabaseInitializer
{
    public static async Task InitializeKdsDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db = scope.ServiceProvider.GetRequiredService<KdsDbContext>();

        await db.Database.MigrateAsync();

        if (environment.IsDevelopment())
        {
            await SeedDemoKdsAsync(db);
        }
    }

    private static async Task SeedDemoKdsAsync(KdsDbContext db)
    {
        var stationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0003");
        var brandId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");
        var productId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006");
        var now = DateTimeOffset.UtcNow;

        var station = await db.KitchenStations.FirstOrDefaultAsync(entity => entity.Id == stationId);
        if (station is null)
        {
            db.KitchenStations.Add(KitchenStation.Create(
                stationId,
                "GRILL",
                "Grill",
                "#2f7d57",
                isActive: true,
                now));
        }
        else
        {
            station.ApplyCatalogChange("GRILL", "Grill", "#2f7d57", isActive: true, now);
        }

        var route = await db.ProductStationRoutes.FirstOrDefaultAsync(entity =>
            entity.BrandId == brandId && entity.ProductId == productId);
        if (route is null)
        {
            db.ProductStationRoutes.Add(ProductStationRouteSnapshot.Create(
                brandId,
                productId,
                stationId,
                "GRILL",
                now));
        }
        else
        {
            route.ChangeStation(stationId, "GRILL", now);
        }

        await db.SaveChangesAsync();
    }
}
