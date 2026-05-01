using DarkKitchen.Catalog.Features.Features;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.ProductStationRoutes;

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
        var now = DateTimeOffset.UtcNow;
        if (route is null)
        {
            route = ProductStationRoute.Create(productId, station.Id, now);
            db.ProductStationRoutes.Add(route);
        }
        else
        {
            route.ChangeStation(station.Id, now);
        }

        product.Touch(now);

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
