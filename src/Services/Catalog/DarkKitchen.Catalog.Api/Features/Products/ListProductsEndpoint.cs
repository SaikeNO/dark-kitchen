using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class ListProductsEndpoint
{
    public static async Task<IReadOnlyList<Response>> HandleAsync(
        Guid? brandId,
        CatalogDbContext db,
        CancellationToken ct)
    {
        var query = db.Products
            .AsNoTracking()
            .Include(product => product.Recipe)
            .ThenInclude(recipe => recipe!.Items)
            .Include(product => product.StationRoute)
            .ThenInclude(route => route!.Station)
            .AsQueryable();

        if (brandId.HasValue)
        {
            query = query.Where(product => product.BrandId == brandId.Value);
        }

        var products = await query
            .OrderBy(product => product.Name)
            .ToArrayAsync(ct);

        return products.Select(Response.FromProduct).ToArray();
    }

    public sealed record Response(
        Guid Id,
        Guid BrandId,
        Guid CategoryId,
        string Name,
        string? Description,
        decimal Price,
        string Currency,
        bool IsActive,
        Guid? StationId,
        string? StationCode,
        int RecipeItemCount)
    {
        public static Response FromProduct(Product product)
        {
            return new Response(
                product.Id,
                product.BrandId,
                product.CategoryId,
                product.Name,
                product.Description,
                product.Price,
                product.Currency,
                product.IsActive,
                product.StationRoute?.StationId,
                product.StationRoute?.Station?.Code,
                product.Recipe?.Items.Count ?? 0);
        }
    }
}
