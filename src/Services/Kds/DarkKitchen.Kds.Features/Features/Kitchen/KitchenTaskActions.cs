using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.Features.Features.Kitchen;

public enum KitchenTaskActionError
{
    None,
    NotFound,
    Conflict
}

public sealed record KitchenTaskActionResult(
    KitchenTaskActionError Error,
    KitchenTaskResponse? Response,
    object? IntegrationEvent,
    string? ErrorMessage)
{
    public bool ShouldNotify => Error == KitchenTaskActionError.None && Response is not null && IntegrationEvent is not null;
}

public static class KitchenTaskActions
{
    public static async Task<KitchenTaskActionResult> StartAsync(
        Guid taskId,
        KdsDbContext db,
        CancellationToken ct)
    {
        var task = await LoadTaskAsync(taskId, db, ct);
        if (task is null)
        {
            return NotFound();
        }

        if (task.Status == KitchenTaskStatus.RoutingMissing)
        {
            return Conflict("Task has no station routing.");
        }

        var now = DateTimeOffset.UtcNow;
        object? integrationEvent = null;
        if (task.Start(now))
        {
            task.Ticket!.RefreshStatus(now);
            integrationEvent = KdsEventFactory.ItemPreparationStarted(task, now);
        }

        return Success(task, integrationEvent);
    }

    public static async Task<KitchenTaskActionResult> CompleteAsync(
        Guid taskId,
        KdsDbContext db,
        CancellationToken ct)
    {
        var task = await LoadTaskAsync(taskId, db, ct);
        if (task is null)
        {
            return NotFound();
        }

        if (task.Status is KitchenTaskStatus.Pending or KitchenTaskStatus.RoutingMissing)
        {
            return Conflict("Task must be in progress before completion.");
        }

        var now = DateTimeOffset.UtcNow;
        object? integrationEvent = null;
        if (task.Complete(now))
        {
            task.Ticket!.RefreshStatus(now);
            integrationEvent = KdsEventFactory.ItemPreparationCompleted(task, now);
        }

        return Success(task, integrationEvent);
    }

    private static async Task<KitchenTask?> LoadTaskAsync(
        Guid taskId,
        KdsDbContext db,
        CancellationToken ct)
    {
        return await db.KitchenTasks
            .Include(task => task.Ticket)
            .ThenInclude(ticket => ticket!.Tasks)
            .FirstOrDefaultAsync(task => task.Id == taskId, ct);
    }

    private static KitchenTaskActionResult Success(KitchenTask task, object? integrationEvent)
    {
        return new KitchenTaskActionResult(
            KitchenTaskActionError.None,
            KitchenTaskResponse.FromTask(task),
            integrationEvent,
            null);
    }

    private static KitchenTaskActionResult NotFound()
    {
        return new KitchenTaskActionResult(KitchenTaskActionError.NotFound, null, null, null);
    }

    private static KitchenTaskActionResult Conflict(string message)
    {
        return new KitchenTaskActionResult(KitchenTaskActionError.Conflict, null, null, message);
    }
}
