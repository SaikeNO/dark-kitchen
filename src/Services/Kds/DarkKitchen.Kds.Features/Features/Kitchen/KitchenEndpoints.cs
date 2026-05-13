using DarkKitchen.Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Kds.Features.Features.Kitchen;

public static class KitchenEndpoints
{
    public static IEndpointRouteBuilder MapKitchenEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/kitchen")
            .RequireAuthorization("ops.operator");

        group.MapGet("/stations", ListStationsAsync);
        group.MapGet("/stations/{stationId:guid}/tasks", ListStationTasksAsync);
        group.MapPost("/tasks/{taskId:guid}/start", StartTaskAsync);
        group.MapPost("/tasks/{taskId:guid}/done", CompleteTaskAsync);

        return app;
    }

    private static async Task<IResult> ListStationsAsync(KdsDbContext db, CancellationToken ct)
    {
        var stations = await db.KitchenStations
            .AsNoTracking()
            .Where(station => station.IsActive)
            .OrderBy(station => station.Name)
            .ToArrayAsync(ct);

        return Results.Ok(stations.Select(StationResponse.FromStation).ToArray());
    }

    private static async Task<IResult> ListStationTasksAsync(
        Guid stationId,
        KdsDbContext db,
        CancellationToken ct)
    {
        var tasks = await db.KitchenTasks
            .AsNoTracking()
            .Include(task => task.Ticket)
            .Where(task => task.StationId == stationId
                && (task.Status == KitchenTaskStatus.Pending || task.Status == KitchenTaskStatus.InProgress))
            .OrderBy(task => task.CreatedAt)
            .ToArrayAsync(ct);

        return Results.Ok(tasks.Select(KitchenTaskResponse.FromTask).ToArray());
    }

    private static async Task<IResult> StartTaskAsync(
        Guid taskId,
        IDbContextOutbox<KdsDbContext> outbox,
        IHubContext<KitchenHub> hub,
        CancellationToken ct)
    {
        var result = await KitchenTaskActions.StartAsync(taskId, outbox.DbContext, ct);
        return await PersistAndNotifyAsync(result, outbox, hub, ct);
    }

    private static async Task<IResult> CompleteTaskAsync(
        Guid taskId,
        IDbContextOutbox<KdsDbContext> outbox,
        IHubContext<KitchenHub> hub,
        CancellationToken ct)
    {
        var result = await KitchenTaskActions.CompleteAsync(taskId, outbox.DbContext, ct);
        return await PersistAndNotifyAsync(result, outbox, hub, ct);
    }

    private static async Task<IResult> PersistAndNotifyAsync(
        KitchenTaskActionResult result,
        IDbContextOutbox<KdsDbContext> outbox,
        IHubContext<KitchenHub> hub,
        CancellationToken ct)
    {
        if (result.Error == KitchenTaskActionError.NotFound)
        {
            return Results.NotFound();
        }

        if (result.Error == KitchenTaskActionError.Conflict)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status409Conflict);
        }

        await PublishAsync(result.IntegrationEvent, outbox);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        if (result.ShouldNotify)
        {
            await KitchenTaskNotifier.NotifyTaskChangedAsync(hub, result.Response!, ct);
        }

        return Results.Ok(result.Response);
    }

    private static async Task PublishAsync(object? integrationEvent, IDbContextOutbox<KdsDbContext> outbox)
    {
        switch (integrationEvent)
        {
            case IntegrationEventEnvelope<ItemPreparationStarted> started:
                await outbox.PublishAsync(started);
                break;
            case IntegrationEventEnvelope<ItemPreparationCompleted> completed:
                await outbox.PublishAsync(completed);
                break;
            case null:
                break;
            default:
                throw new InvalidOperationException($"Unsupported KDS integration event: {integrationEvent.GetType()}.");
        }
    }
}
