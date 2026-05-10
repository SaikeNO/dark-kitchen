using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.Features.Features.Catalog;

public static class ProductStationRoutingChangedHandler
{
    public static async Task Handle(
        IntegrationEventEnvelope<ProductStationRoutingChanged> envelope,
        KdsDbContext db,
        CancellationToken ct)
    {
        var payload = envelope.Payload;
        var snapshot = await db.ProductStationRoutes.FirstOrDefaultAsync(
            route => route.BrandId == payload.BrandId && route.ProductId == payload.ProductId,
            ct);

        if (payload.StationId is null || string.IsNullOrWhiteSpace(payload.StationCode))
        {
            if (snapshot is not null)
            {
                db.ProductStationRoutes.Remove(snapshot);
                await db.SaveChangesAsync(ct);
            }

            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (snapshot is null)
        {
            db.ProductStationRoutes.Add(ProductStationRouteSnapshot.Create(
                payload.BrandId,
                payload.ProductId,
                payload.StationId.Value,
                payload.StationCode,
                now));
        }
        else
        {
            snapshot.ChangeStation(payload.StationId.Value, payload.StationCode, now);
        }

        await db.SaveChangesAsync(ct);
    }
}
