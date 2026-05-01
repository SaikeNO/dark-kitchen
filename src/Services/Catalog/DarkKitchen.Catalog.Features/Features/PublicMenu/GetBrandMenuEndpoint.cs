using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.PublicMenu;

public static class GetBrandMenuEndpoint
{
    public static async Task<IResult> HandleAsync(Guid brandId, CatalogDbContext db, CancellationToken ct)
    {
        var brand = await db.Brands
            .AsNoTracking()
            .Where(entity => entity.Id == brandId && entity.IsActive)
            .Select(entity => new { entity.Id, entity.Name })
            .FirstOrDefaultAsync(ct);

        if (brand is null)
        {
            return Results.NotFound();
        }

        var categories = await db.Categories
            .AsNoTracking()
            .Where(category => category.BrandId == brandId && category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.SortOrder,
                category.Products
                    .Where(product => product.IsActive)
                    .OrderBy(product => product.Name)
                    .Select(product => new ProductResponse(
                        product.Id,
                        product.Name,
                        product.Description,
                        product.Price,
                        product.Currency))
                    .ToArray()))
            .ToArrayAsync(ct);

        return Results.Ok(new Response(brand.Id, brand.Name, categories));
    }

    public sealed record Response(
        Guid BrandId,
        string BrandName,
        IReadOnlyList<CategoryResponse> Categories);

    public sealed record CategoryResponse(
        Guid Id,
        string Name,
        int SortOrder,
        IReadOnlyList<ProductResponse> Products);

    public sealed record ProductResponse(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        string Currency);
}
