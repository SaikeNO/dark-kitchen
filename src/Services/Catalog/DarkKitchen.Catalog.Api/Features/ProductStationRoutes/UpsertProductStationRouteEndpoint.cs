using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class UpsertProductStationRouteEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid productId,
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var db = outbox.DbContext;
        var product = await db.Products.FirstOrDefaultAsync(entity => entity.Id == productId, ct);
        if (product is null)
        {
            return Results.NotFound();
        }

        var station = await db.Stations.FirstOrDefaultAsync(entity => entity.Id == request.StationId && entity.IsActive, ct);
        if (station is null)
        {
            return ApiValidation.Problem(("stationId", "Active station is required."));
        }

        var route = await db.ProductStationRoutes.FirstOrDefaultAsync(entity => entity.ProductId == productId, ct);
        if (route is null)
        {
            route = new ProductStationRoute { ProductId = productId };
            db.ProductStationRoutes.Add(route);
        }

        route.StationId = station.Id;
        route.UpdatedAt = DateTimeOffset.UtcNow;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await outbox.PublishAsync(CatalogEventFactory.ProductStationRoutingChanged(product, station, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(new Response(productId, station.Id, station.Code));
    }

    public sealed record Request(Guid StationId);

    public sealed record Response(
        Guid ProductId,
        Guid StationId,
        string StationCode);
}
