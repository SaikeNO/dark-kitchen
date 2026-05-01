using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class UpdateProductEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid productId,
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var db = outbox.DbContext;
        var validation = await ProductRequestValidation.ValidateAsync(
            request.BrandId,
            request.CategoryId,
            request.Name,
            request.Price,
            request.Currency,
            db,
            ct);
        if (validation is not null)
        {
            return validation;
        }

        var product = await db.Products
            .Include(entity => entity.Recipe)
            .ThenInclude(recipe => recipe!.Items)
            .Include(entity => entity.StationRoute)
            .ThenInclude(route => route!.Station)
            .FirstOrDefaultAsync(entity => entity.Id == productId, ct);

        if (product is null)
        {
            return Results.NotFound();
        }

        var normalizedCurrency = ApiValidation.NormalizeCurrency(request.Currency);
        var priceChanged = product.Price != request.Price
            || !string.Equals(product.Currency, normalizedCurrency, StringComparison.Ordinal);

        product.BrandId = request.BrandId;
        product.CategoryId = request.CategoryId;
        product.Name = request.Name.Trim();
        product.Description = ApiValidation.TrimOptional(request.Description);
        product.Price = request.Price;
        product.Currency = normalizedCurrency;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await outbox.PublishAsync(CatalogEventFactory.MenuItemChanged(product, httpContext));
        if (priceChanged)
        {
            await outbox.PublishAsync(CatalogEventFactory.ProductPriceChanged(product, httpContext));
        }

        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(Response.FromProduct(product));
    }

    public sealed record Request(
        Guid BrandId,
        Guid CategoryId,
        string Name,
        string? Description,
        decimal Price,
        string Currency);

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
