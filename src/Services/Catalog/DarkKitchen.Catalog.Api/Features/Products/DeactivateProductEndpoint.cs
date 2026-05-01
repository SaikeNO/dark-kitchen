using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class DeactivateProductEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid productId,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var product = await outbox.DbContext.Products.FirstOrDefaultAsync(entity => entity.Id == productId, ct);
        if (product is null)
        {
            return Results.NotFound();
        }

        product.IsActive = false;
        product.UpdatedAt = DateTimeOffset.UtcNow;
        await outbox.PublishAsync(CatalogEventFactory.MenuItemChanged(product, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(new Response(
            product.Id,
            product.BrandId,
            product.CategoryId,
            product.Name,
            product.Description,
            product.Price,
            product.Currency,
            product.IsActive,
            null,
            null,
            0));
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
        int RecipeItemCount);
}
