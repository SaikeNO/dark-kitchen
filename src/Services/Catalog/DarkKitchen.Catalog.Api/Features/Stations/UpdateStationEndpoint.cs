using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class UpdateStationEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid stationId,
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return validation;
        }

        var station = await outbox.DbContext.Stations.FirstOrDefaultAsync(entity => entity.Id == stationId, ct);
        if (station is null)
        {
            return Results.NotFound();
        }

        station.Code = request.Code.Trim().ToUpperInvariant();
        station.Name = request.Name.Trim();
        station.DisplayColor = request.DisplayColor.Trim();
        station.IsActive = request.IsActive;
        station.UpdatedAt = DateTimeOffset.UtcNow;
        await outbox.PublishAsync(CatalogEventFactory.StationChanged(station, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(Response.FromStation(station));
    }

    private static IResult? Validate(Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return ApiValidation.Problem(("code", "Station code is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiValidation.Problem(("name", "Station name is required."));
        }

        return string.IsNullOrWhiteSpace(request.DisplayColor)
            ? ApiValidation.Problem(("displayColor", "Station display color is required."))
            : null;
    }

    public sealed record Request(
        string Code,
        string Name,
        string DisplayColor,
        bool IsActive);

    public sealed record Response(
        Guid Id,
        string Code,
        string Name,
        string DisplayColor,
        bool IsActive)
    {
        public static Response FromStation(Station station)
        {
            return new Response(station.Id, station.Code, station.Name, station.DisplayColor, station.IsActive);
        }
    }
}
