using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Infrastructure.Persistence;

public static class CatalogDatabaseInitializer
{
    public static async Task InitializeCatalogDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>());

        if (environment.IsDevelopment())
        {
            await SeedDemoUsersAsync(
                scope.ServiceProvider.GetRequiredService<UserManager<CatalogUser>>());

            await SeedDemoCatalogAsync(db);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in new[] { CatalogRoles.Manager, CatalogRoles.Operator })
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            ThrowIfFailed(result, $"Could not create role '{roleName}'.");
        }
    }

    private static async Task SeedDemoUsersAsync(UserManager<CatalogUser> userManager)
    {
        await EnsureUserAsync(userManager, CatalogDemoAccounts.ManagerEmail, CatalogRoles.Manager);
        await EnsureUserAsync(userManager, CatalogDemoAccounts.OperatorEmail, CatalogRoles.Operator);
    }

    private static async Task EnsureUserAsync(UserManager<CatalogUser> userManager, string email, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new CatalogUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            ThrowIfFailed(
                await userManager.CreateAsync(user, CatalogDemoAccounts.Password),
                $"Could not create demo user '{email}'.");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            ThrowIfFailed(
                await userManager.AddToRoleAsync(user, role),
                $"Could not assign demo user '{email}' to role '{role}'.");
        }
    }

    private static async Task SeedDemoCatalogAsync(CatalogDbContext db)
    {
        if (await db.Brands.AnyAsync())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var brand = Brand.Create(
            name: "Burger Ghost",
            description: "Demo brand for local Dark Kitchen workflows.",
            logoUrl: null,
            isActive: true,
            now: now,
            id: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001"));

        var category = Category.Create(
            brandId: brand.Id,
            name: "Burgery",
            sortOrder: 10,
            isActive: true,
            now: now,
            id: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0002"));

        var station = Station.Create(
            code: "GRILL",
            name: "Grill",
            displayColor: "#2f7d57",
            isActive: true,
            now: now,
            id: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0003"));

        var bun = Ingredient.Create(
            name: "Bulka burgerowa",
            unit: "szt",
            isActive: true,
            now: now,
            id: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0004"));

        var patty = Ingredient.Create(
            name: "Kotlet wolowy",
            unit: "g",
            isActive: true,
            now: now,
            id: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0005"));

        var product = Product.Create(
            brandId: brand.Id,
            categoryId: category.Id,
            name: "Classic Smash",
            description: "Demo burger for the MVP catalog.",
            price: 32.90m,
            currency: "PLN",
            now: now,
            id: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006"),
            isActive: true);

        db.Brands.Add(brand);
        db.Categories.Add(category);
        db.Stations.Add(station);
        db.Ingredients.AddRange(bun, patty);
        db.Products.Add(product);
        var recipe = Recipe.Create(product.Id, now);
        recipe.ReplaceItems(
            [
                RecipeItem.Create(product.Id, bun.Id, 1),
                RecipeItem.Create(product.Id, patty.Id, 180)
            ],
            now);
        db.Recipes.Add(recipe);
        db.ProductStationRoutes.Add(ProductStationRoute.Create(product.Id, station.Id, now));

        await db.SaveChangesAsync();
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {errors}");
    }
}
