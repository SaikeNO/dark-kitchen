using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Categories;

public static class ListCategoriesEndpoint
{
    public static async Task<IReadOnlyList<Response>> HandleAsync(
        Guid? brandId,
        CatalogDbContext db,
        CancellationToken ct)
    {
        var query = db.Categories.AsNoTracking();
        if (brandId.HasValue)
        {
            query = query.Where(category => category.BrandId == brandId.Value);
        }

        return await query
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new Response(category.Id, category.BrandId, category.Name, category.SortOrder, category.IsActive))
            .ToArrayAsync(ct);
    }

    public sealed record Response(
        Guid Id,
        Guid BrandId,
        string Name,
        int SortOrder,
        bool IsActive);
}
