using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Infrastructure.Persistence;

public static class InventoryDatabaseInitializer
{
    public static async Task InitializeInventoryDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        await db.Database.MigrateAsync();

        if (environment.IsDevelopment())
        {
            await SeedDemoInventoryAsync(db);
        }
    }

    private static async Task SeedDemoInventoryAsync(InventoryDbContext db)
    {
        var bunId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0004");
        var pattyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0005");
        var productId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006");
        var brandId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");

        if (await db.WarehouseItems.AnyAsync(item => item.Id == bunId))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        db.WarehouseItems.AddRange(
            WarehouseItem.Create(bunId, "Bulka burgerowa", "szt", now, onHandQuantity: 40, minSafetyLevel: 12),
            WarehouseItem.Create(pattyId, "Kotlet wolowy", "g", now, onHandQuantity: 6000, minSafetyLevel: 2500));

        var recipe = RecipeSnapshot.Create(productId, brandId, now);
        recipe.ReplaceItems(
            brandId,
            [
                RecipeSnapshotItem.Create(productId, bunId, "Bulka burgerowa", "szt", 1),
                RecipeSnapshotItem.Create(productId, pattyId, "Kotlet wolowy", "g", 180)
            ],
            now);
        db.RecipeSnapshots.Add(recipe);

        await db.SaveChangesAsync();
    }
}
