using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class ListBrandsEndpoint
{
    public static async Task<IReadOnlyList<Response>> HandleAsync(CatalogDbContext db, CancellationToken ct)
    {
        return await db.Brands
            .AsNoTracking()
            .OrderBy(brand => brand.Name)
            .Select(brand => new Response(brand.Id, brand.Name, brand.Description, brand.LogoUrl, brand.IsActive))
            .ToArrayAsync(ct);
    }

    public sealed record Response(
        Guid Id,
        string Name,
        string? Description,
        string? LogoUrl,
        bool IsActive);
}
