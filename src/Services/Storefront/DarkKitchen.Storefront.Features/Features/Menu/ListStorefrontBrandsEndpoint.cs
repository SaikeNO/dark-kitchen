using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Menu;

public static class ListStorefrontBrandsEndpoint
{
    public static async Task<IResult> HandleAsync(StorefrontDbContext db, CancellationToken ct)
    {
        var brands = await db.BrandSites
            .AsNoTracking()
            .Where(brand => brand.IsActive)
            .OrderBy(brand => brand.Name)
            .Select(brand => new StorefrontContextResponse(
                brand.BrandId,
                brand.Name,
                brand.Description,
                brand.LogoUrl,
                brand.HeroTitle,
                brand.HeroSubtitle,
                new StorefrontThemeResponse(
                    brand.PrimaryColor,
                    brand.AccentColor,
                    brand.BackgroundColor,
                    brand.TextColor)))
            .ToListAsync(ct);

        return Results.Ok(brands);
    }
}
