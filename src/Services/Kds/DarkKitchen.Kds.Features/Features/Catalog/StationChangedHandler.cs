using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.Features.Features.Catalog;

public static class StationChangedHandler
{
    public static async Task Handle(
        IntegrationEventEnvelope<StationChanged> envelope,
        KdsDbContext db,
        CancellationToken ct)
    {
        var payload = envelope.Payload;
        var station = await db.KitchenStations.FirstOrDefaultAsync(entity => entity.Id == payload.StationId, ct);
        var now = DateTimeOffset.UtcNow;

        if (station is null)
        {
            db.KitchenStations.Add(KitchenStation.Create(
                payload.StationId,
                payload.Code,
                payload.Name,
                payload.DisplayColor,
                payload.IsActive,
                now));
        }
        else
        {
            station.ApplyCatalogChange(payload.Code, payload.Name, payload.DisplayColor, payload.IsActive, now);
        }

        await db.SaveChangesAsync(ct);
    }
}
