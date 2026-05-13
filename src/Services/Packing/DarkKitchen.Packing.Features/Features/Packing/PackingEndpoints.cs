using DarkKitchen.Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Packing.Features.Features.Packing;

public static class PackingEndpoints
{
    public static IEndpointRouteBuilder MapPackingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/packing")
            .RequireAuthorization("ops.operator");

        group.MapGet("/manifests", ListManifestsAsync);
        group.MapPost("/manifests/{manifestId:guid}/issued", IssueManifestAsync);

        return app;
    }

    private static async Task<IResult> ListManifestsAsync(PackingDbContext db, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var manifests = await db.PackingManifests
            .AsNoTracking()
            .Include(manifest => manifest.Items)
            .Where(manifest => manifest.Status != PackingManifestStatus.Issued)
            .OrderBy(manifest => manifest.CreatedAt)
            .ToArrayAsync(ct);

        return Results.Ok(manifests
            .Select(manifest => PackingManifestResponse.FromManifest(manifest, now, PackingOptions.DelayThreshold))
            .ToArray());
    }

    private static async Task<IResult> IssueManifestAsync(
        Guid manifestId,
        IssueManifestRequest request,
        IDbContextOutbox<PackingDbContext> outbox,
        IHubContext<PackingHub> hub,
        CancellationToken ct)
    {
        var result = await PackingManifestActions.IssueAsync(manifestId, request.PickupCode, outbox, ct);
        if (result.Error == PackingActionError.NotFound)
        {
            return Results.NotFound();
        }

        if (result.Error == PackingActionError.Conflict)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status409Conflict);
        }

        await OrderAcceptedHandler.PublishAsync(result.IntegrationEvent, outbox);
        await OrderAcceptedHandler.PublishAsync(result.SecondaryIntegrationEvent, outbox);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);
        await PackingManifestNotifier.NotifyManifestChangedAsync(hub, result.Manifest!, ct);

        return Results.Ok(PackingManifestResponse.FromManifest(
            result.Manifest!,
            DateTimeOffset.UtcNow,
            PackingOptions.DelayThreshold));
    }
}
