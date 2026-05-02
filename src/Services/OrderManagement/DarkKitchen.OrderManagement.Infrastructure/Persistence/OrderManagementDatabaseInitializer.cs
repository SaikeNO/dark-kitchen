using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Infrastructure.Persistence;

public static class OrderManagementDatabaseInitializer
{
    public static async Task InitializeOrderManagementDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();

        await db.Database.MigrateAsync();

        if (environment.IsDevelopment())
        {
            await SeedDemoMenuAsync(db);
        }
    }

    private static async Task SeedDemoMenuAsync(OrderManagementDbContext db)
    {
        var productId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006");
        if (await db.MenuItemSnapshots.AnyAsync(item => item.MenuItemId == productId))
        {
            return;
        }

        db.MenuItemSnapshots.Add(MenuItemSnapshot.Create(
            menuItemId: productId,
            brandId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001"),
            categoryId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0002"),
            name: "Classic Smash",
            description: "Demo burger for the MVP order flow.",
            price: 32.90m,
            currency: "PLN",
            isActive: true,
            now: DateTimeOffset.UtcNow));

        await db.SaveChangesAsync();
    }
}
