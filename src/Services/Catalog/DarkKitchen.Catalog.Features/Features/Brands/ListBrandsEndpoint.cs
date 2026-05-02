using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Brands;

public static class ListBrandsEndpoint
{
    public static async Task<IReadOnlyList<Response>> HandleAsync(CatalogDbContext db, CancellationToken ct)
    {
        return await db.Brands
            .AsNoTracking()
            .OrderBy(brand => brand.Name)
            .Select(brand => new Response(
                brand.Id,
                brand.Name,
                brand.Description,
                brand.LogoUrl,
                brand.Domains,
                brand.HeroTitle,
                brand.HeroSubtitle,
                brand.PrimaryColor,
                brand.AccentColor,
                brand.BackgroundColor,
                brand.TextColor,
                brand.IsActive))
            .ToArrayAsync(ct);
    }

    public sealed record Response(
        Guid Id,
        string Name,
        string? Description,
        string? LogoUrl,
        IReadOnlyList<string> Domains,
        string? HeroTitle,
        string? HeroSubtitle,
        string PrimaryColor,
        string AccentColor,
        string BackgroundColor,
        string TextColor,
        bool IsActive);
}
