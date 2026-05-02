using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.PublicMenu;

public static class GetBrandMenuEndpoint
{
    public static async Task<IResult> HandleAsync(Guid brandId, CatalogDbContext db, CancellationToken ct)
    {
        var brand = await db.Brands
            .AsNoTracking()
            .Where(entity => entity.Id == brandId && entity.IsActive)
            .Select(entity => new
            {
                entity.Id,
                entity.Name,
                entity.Description,
                entity.LogoUrl,
                entity.HeroTitle,
                entity.HeroSubtitle,
                entity.PrimaryColor,
                entity.AccentColor,
                entity.BackgroundColor,
                entity.TextColor
            })
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
                        product.ImageUrl,
                        product.Price,
                        product.Currency))
                    .ToArray()))
            .ToArrayAsync(ct);

        return Results.Ok(new Response(
            brand.Id,
            brand.Name,
            brand.Description,
            brand.LogoUrl,
            brand.HeroTitle,
            brand.HeroSubtitle,
            brand.PrimaryColor,
            brand.AccentColor,
            brand.BackgroundColor,
            brand.TextColor,
            categories));
    }

    public sealed record Response(
        Guid BrandId,
        string BrandName,
        string? BrandDescription,
        string? LogoUrl,
        string? HeroTitle,
        string? HeroSubtitle,
        string PrimaryColor,
        string AccentColor,
        string BackgroundColor,
        string TextColor,
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
        string? ImageUrl,
        decimal Price,
        string Currency);
}
