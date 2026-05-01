using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Stations;

public static class DeactivateStationEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid stationId,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var station = await outbox.DbContext.Stations.FirstOrDefaultAsync(entity => entity.Id == stationId, ct);
        if (station is null)
        {
            return Results.NotFound();
        }

        station.Deactivate(DateTimeOffset.UtcNow);
        await outbox.PublishAsync(CatalogEventFactory.StationChanged(station, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(new Response(station.Id, station.Code, station.Name, station.DisplayColor, station.IsActive));
    }

    public sealed record Response(
        Guid Id,
        string Code,
        string Name,
        string DisplayColor,
        bool IsActive);
}
