using DarkKitchen.Catalog.Features.Features;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Products;

public static class CreateProductEndpoint
{
    public static async Task<IResult> HandleAsync(
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

        var product = Product.Create(
            request.BrandId,
            request.CategoryId,
            request.Name.Trim(),
            ApiValidation.TrimOptional(request.Description),
            ApiValidation.TrimOptional(request.ImageUrl),
            request.Price,
            ApiValidation.NormalizeCurrency(request.Currency),
            DateTimeOffset.UtcNow);

        db.Products.Add(product);
        await outbox.PublishAsync(CatalogEventFactory.MenuItemChanged(product, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Created($"/api/admin/products/{product.Id}", Response.FromProduct(product));
    }

    public sealed record Request(
        Guid BrandId,
        Guid CategoryId,
        string Name,
        string? Description,
        string? ImageUrl,
        decimal Price,
        string Currency);

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
