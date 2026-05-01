using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api;

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
        var brand = new Brand
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001"),
            Name = "Burger Ghost",
            Description = "Demo brand for local Dark Kitchen workflows.",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var category = new Category
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0002"),
            BrandId = brand.Id,
            Name = "Burgery",
            SortOrder = 10,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var station = new Station
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0003"),
            Code = "GRILL",
            Name = "Grill",
            DisplayColor = "#2f7d57",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var bun = new Ingredient
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0004"),
            Name = "Bulka burgerowa",
            Unit = "szt",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var patty = new Ingredient
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0005"),
            Name = "Kotlet wolowy",
            Unit = "g",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var product = new Product
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006"),
            BrandId = brand.Id,
            CategoryId = category.Id,
            Name = "Classic Smash",
            Description = "Demo burger for the MVP catalog.",
            Price = 32.90m,
            Currency = "PLN",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Brands.Add(brand);
        db.Categories.Add(category);
        db.Stations.Add(station);
        db.Ingredients.AddRange(bun, patty);
        db.Products.Add(product);
        db.Recipes.Add(new Recipe
        {
            ProductId = product.Id,
            UpdatedAt = now,
            Items =
            [
                new RecipeItem { ProductId = product.Id, IngredientId = bun.Id, Quantity = 1 },
                new RecipeItem { ProductId = product.Id, IngredientId = patty.Id, Quantity = 180 }
            ]
        });
        db.ProductStationRoutes.Add(new ProductStationRoute
        {
            ProductId = product.Id,
            StationId = station.Id,
            UpdatedAt = now
        });

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
