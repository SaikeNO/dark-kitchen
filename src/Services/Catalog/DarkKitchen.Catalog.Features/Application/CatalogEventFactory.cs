using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Application;

public static class CatalogEventFactory
{
    public static IntegrationEventEnvelope<BrandChanged> BrandChanged(Brand brand, HttpContext httpContext)
    {
        return CreateEnvelope(
            brand.Id,
            new BrandChanged(
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
                brand.IsActive),
            httpContext);
    }

    public static IntegrationEventEnvelope<CategoryChanged> CategoryChanged(Category category, HttpContext httpContext)
    {
        return CreateEnvelope(
            category.BrandId,
            new CategoryChanged(
                category.Id,
                category.BrandId,
                category.Name,
                category.SortOrder,
                category.IsActive),
            httpContext);
    }

    public static IntegrationEventEnvelope<MenuItemChanged> MenuItemChanged(Product product, HttpContext httpContext)
    {
        return CreateEnvelope(
            product.BrandId,
            new MenuItemChanged(
                product.Id,
                product.BrandId,
                product.CategoryId,
                product.Name,
                product.Description,
                product.ImageUrl,
                product.Price,
                product.Currency,
                product.IsActive),
            httpContext);
    }

    public static IntegrationEventEnvelope<ProductPriceChanged> ProductPriceChanged(Product product, HttpContext httpContext)
    {
        return CreateEnvelope(
            product.BrandId,
            new ProductPriceChanged(
                product.Id,
                product.BrandId,
                product.Price,
                product.Currency),
            httpContext);
    }

    public static async Task<IntegrationEventEnvelope<RecipeChanged>> RecipeChangedAsync(
        CatalogDbContext db,
        Product product,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var items = await db.RecipeItems
            .AsNoTracking()
            .Where(item => item.ProductId == product.Id)
            .Include(item => item.Ingredient)
            .OrderBy(item => item.Ingredient!.Name)
            .Select(item => new RecipeChangedItem(
                item.IngredientId,
                item.Ingredient!.Name,
                item.Ingredient.Unit,
                item.Quantity))
            .ToArrayAsync(cancellationToken);

        return CreateEnvelope(
            product.BrandId,
            new RecipeChanged(product.Id, product.BrandId, items),
            httpContext);
    }

    public static IntegrationEventEnvelope<RecipeChanged> RecipeChanged(
        Product product,
        IReadOnlyList<RecipeChangedItem> items,
        HttpContext httpContext)
    {
        return CreateEnvelope(
            product.BrandId,
            new RecipeChanged(product.Id, product.BrandId, items),
            httpContext);
    }

    public static IntegrationEventEnvelope<StationChanged> StationChanged(Station station, HttpContext httpContext)
    {
        return CreateEnvelope(
            "catalog-global",
            new StationChanged(
                station.Id,
                station.Code,
                station.Name,
                station.DisplayColor,
                station.IsActive),
            httpContext);
    }

    public static async Task<IntegrationEventEnvelope<ProductStationRoutingChanged>> ProductStationRoutingChangedAsync(
        CatalogDbContext db,
        Product product,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var route = await db.ProductStationRoutes
            .AsNoTracking()
            .Include(productRoute => productRoute.Station)
            .FirstOrDefaultAsync(productRoute => productRoute.ProductId == product.Id, cancellationToken);

        return CreateEnvelope(
            product.BrandId,
            new ProductStationRoutingChanged(
                product.Id,
                product.BrandId,
                route?.StationId,
                route?.Station?.Code),
            httpContext);
    }

    public static IntegrationEventEnvelope<ProductStationRoutingChanged> ProductStationRoutingChanged(
        Product product,
        Station? station,
        HttpContext httpContext)
    {
        return CreateEnvelope(
            product.BrandId,
            new ProductStationRoutingChanged(
                product.Id,
                product.BrandId,
                station?.Id,
                station?.Code),
            httpContext);
    }

    private static IntegrationEventEnvelope<TPayload> CreateEnvelope<TPayload>(
        Guid brandId,
        TPayload payload,
        HttpContext httpContext)
    {
        return CreateEnvelope(brandId.ToString("D"), payload, httpContext);
    }

    private static IntegrationEventEnvelope<TPayload> CreateEnvelope<TPayload>(
        string brandId,
        TPayload payload,
        HttpContext httpContext)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            correlationId: GetCorrelationId(httpContext),
            causationId: null,
            brandId: brandId,
            payload: payload);
    }

    private static Guid GetCorrelationId(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var header)
            && Guid.TryParse(header.FirstOrDefault(), out var correlationId)
            && correlationId != Guid.Empty)
        {
            return correlationId;
        }

        return Guid.NewGuid();
    }
}
