using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Application;

public static class BrandResolver
{
    public static async Task<BrandSiteSnapshot?> ResolveAsync(HttpContext httpContext, StorefrontDbContext db, CancellationToken ct)
    {
        var brandId = ReadBrandId(httpContext);
        if (brandId is not null)
        {
            return await db.BrandSites
                .AsNoTracking()
                .FirstOrDefaultAsync(brand => brand.BrandId == brandId.Value && brand.IsActive, ct);
        }

        var host = httpContext.Request.Host.Host.Trim().ToLowerInvariant();
        return await db.BrandSites
            .AsNoTracking()
            .Where(brand => brand.IsActive && brand.Domains.Contains(host))
            .OrderBy(brand => brand.Name)
            .FirstOrDefaultAsync(ct);
    }

    private static Guid? ReadBrandId(HttpContext httpContext)
    {
        if (Guid.TryParse(httpContext.Request.Query["brandId"].FirstOrDefault(), out var queryBrandId)
            && queryBrandId != Guid.Empty)
        {
            return queryBrandId;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Brand-Id", out var header)
            && Guid.TryParse(header.FirstOrDefault(), out var headerBrandId)
            && headerBrandId != Guid.Empty)
        {
            return headerBrandId;
        }

        return null;
    }
}
