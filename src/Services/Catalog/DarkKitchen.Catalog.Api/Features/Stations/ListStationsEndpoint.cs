using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class ListStationsEndpoint
{
    public static async Task<IReadOnlyList<Response>> HandleAsync(CatalogDbContext db, CancellationToken ct)
    {
        return await db.Stations
            .AsNoTracking()
            .OrderBy(station => station.Code)
            .Select(station => new Response(station.Id, station.Code, station.Name, station.DisplayColor, station.IsActive))
            .ToArrayAsync(ct);
    }

    public sealed record Response(
        Guid Id,
        string Code,
        string Name,
        string DisplayColor,
        bool IsActive);
}
