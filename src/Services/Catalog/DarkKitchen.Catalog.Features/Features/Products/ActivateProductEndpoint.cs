using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Products;

public static class ActivateProductEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid productId,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var product = await outbox.DbContext.Products
            .Include(entity => entity.Brand)
            .Include(entity => entity.Category)
            .Include(entity => entity.Recipe)
            .ThenInclude(recipe => recipe!.Items)
            .Include(entity => entity.StationRoute)
            .ThenInclude(route => route!.Station)
            .FirstOrDefaultAsync(entity => entity.Id == productId, ct);

        if (product is null)
        {
            return Results.NotFound();
        }

        var validation = Validate(product);
        if (validation is not null)
        {
            return validation;
        }

        product.Activate(DateTimeOffset.UtcNow);
        await outbox.PublishAsync(CatalogEventFactory.MenuItemChanged(product, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(Response.FromProduct(product));
    }

    private static IResult? Validate(Product product)
    {
        var errors = new List<(string Key, string Error)>();

        if (product.Brand is not { IsActive: true })
        {
            errors.Add(("brandId", "Active brand is required."));
        }

        if (product.Category is not { IsActive: true })
        {
            errors.Add(("categoryId", "Active category is required."));
        }

        if (product.Price <= 0)
        {
            errors.Add(("price", "Price must be greater than zero."));
        }

        if (!string.Equals(product.Currency, "PLN", StringComparison.Ordinal))
        {
            errors.Add(("currency", "Only PLN is supported in the MVP."));
        }

        if (product.Recipe?.Items.Count is null or 0)
        {
            errors.Add(("recipe", "Recipe with at least one ingredient is required."));
        }

        if (product.StationRoute?.Station is not { IsActive: true })
        {
            errors.Add(("stationId", "Active kitchen station route is required."));
        }

        return errors.Count == 0 ? null : ApiValidation.Problem(errors.ToArray());
    }

    public sealed record Response(
        Guid Id,
        Guid BrandId,
        Guid CategoryId,
        string Name,
        string? Description,
        string? ImageUrl,
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
                product.ImageUrl,
                product.Price,
                product.Currency,
                product.IsActive,
                product.StationRoute?.StationId,
                product.StationRoute?.Station?.Code,
                product.Recipe?.Items.Count ?? 0);
        }
    }
}
