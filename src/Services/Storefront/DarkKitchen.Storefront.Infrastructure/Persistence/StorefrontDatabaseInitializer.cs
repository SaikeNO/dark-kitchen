using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Infrastructure.Persistence;

public static class StorefrontDatabaseInitializer
{
    public static async Task InitializeStorefrontDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db = scope.ServiceProvider.GetRequiredService<StorefrontDbContext>();

        await db.Database.MigrateAsync();

        if (environment.IsDevelopment())
        {
            await SeedDemoStorefrontAsync(db);
        }
    }

    private static async Task SeedDemoStorefrontAsync(StorefrontDbContext db)
    {
        var brandId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");
        if (await db.BrandSites.AnyAsync(brand => brand.BrandId == brandId))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        db.BrandSites.Add(BrandSiteSnapshot.Create(
            brandId,
            "Burger Ghost",
            "Demo brand for local Dark Kitchen workflows.",
            null,
            ["localhost", "127.0.0.1"],
            "Smash burgery z ukrytej kuchni",
            "Szybki checkout, gorace menu, gotowe do odbioru.",
            "#dc2626",
            "#ca8a04",
            "#fef2f2",
            "#450a0a",
            true,
            now));

        var categoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0002");
        db.MenuCategories.Add(MenuCategorySnapshot.Create(categoryId, brandId, "Burgery", 10, true, now));
        db.MenuItems.Add(MenuItemSnapshot.Create(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006"),
            brandId,
            categoryId,
            "Classic Smash",
            "Demo burger for the MVP catalog.",
            null,
            32.90m,
            "PLN",
            true,
            now));

        await db.SaveChangesAsync();
    }
}
