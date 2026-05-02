using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Menu;

public static class GetStorefrontMenuEndpoint
{
    public static async Task<IResult> HandleAsync(
        StorefrontDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var brand = await BrandResolver.ResolveAsync(httpContext, db, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        var categories = await db.MenuCategories
            .AsNoTracking()
            .Where(category => category.BrandId == brand.BrandId && category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToArrayAsync(ct);

        var products = await db.MenuItems
            .AsNoTracking()
            .Where(item => item.BrandId == brand.BrandId && item.IsActive)
            .OrderBy(item => item.Name)
            .ToArrayAsync(ct);

        var productsByCategory = products
            .GroupBy(product => product.CategoryId)
            .ToDictionary(group => group.Key, group => group.Select(product => new StorefrontProductResponse(
                product.MenuItemId,
                product.CategoryId,
                product.Name,
                product.Description,
                product.ImageUrl,
                product.Price,
                product.Currency)).ToArray());

        var response = new StorefrontMenuResponse(
            StorefrontMenuMapping.ToContext(brand),
            categories
                .Select(category => new StorefrontCategoryResponse(
                    category.CategoryId,
                    category.Name,
                    category.SortOrder,
                    productsByCategory.GetValueOrDefault(category.CategoryId, [])))
                .Where(category => category.Products.Count > 0)
                .ToArray());

        return Results.Ok(response);
    }
}
