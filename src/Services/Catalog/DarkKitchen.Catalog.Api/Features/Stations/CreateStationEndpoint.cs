using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class CreateStationEndpoint
{
    public static async Task<IResult> HandleAsync(
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

        var now = DateTimeOffset.UtcNow;
        var station = new Station
        {
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            DisplayColor = request.DisplayColor.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        outbox.DbContext.Stations.Add(station);
        await outbox.PublishAsync(CatalogEventFactory.StationChanged(station, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Created($"/api/admin/stations/{station.Id}", Response.FromStation(station));
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
