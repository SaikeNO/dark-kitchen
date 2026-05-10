using DarkKitchen.Contracts.Events;
using DarkKitchen.Kds.Features.Features.Kitchen;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.Features.Features.Orders;

public static class OrderAcceptedHandler
{
    public static async Task Handle(
        IntegrationEventEnvelope<OrderAccepted> envelope,
        KdsDbContext db,
        IHubContext<KitchenHub> hub,
        CancellationToken ct)
    {
        var tasks = await CreateTicketAsync(envelope, db, ct);
        foreach (var task in tasks)
        {
            await KitchenTaskNotifier.NotifyTaskChangedAsync(hub, task, ct);
        }
    }

    public static async Task<IReadOnlyList<KitchenTaskResponse>> CreateTicketAsync(
        IntegrationEventEnvelope<OrderAccepted> envelope,
        KdsDbContext db,
        CancellationToken ct)
    {
        if (!Guid.TryParse(envelope.BrandId, out var brandId))
        {
            brandId = Guid.Empty;
        }

        if (await db.KitchenTickets.AnyAsync(ticket => ticket.OrderId == envelope.Payload.OrderId, ct))
        {
            return [];
        }

        var productIds = envelope.Payload.Items.Select(item => item.MenuItemId).Distinct().ToArray();
        var routes = await db.ProductStationRoutes
            .AsNoTracking()
            .Where(route => route.BrandId == brandId && productIds.Contains(route.ProductId))
            .ToDictionaryAsync(route => route.ProductId, ct);
        var stationIds = routes.Values.Select(route => route.StationId).Distinct().ToArray();
        var stations = await db.KitchenStations
            .AsNoTracking()
            .Where(station => stationIds.Contains(station.Id))
            .ToDictionaryAsync(station => station.Id, ct);

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (await db.KitchenTickets.AnyAsync(ticket => ticket.OrderId == envelope.Payload.OrderId, ct))
        {
            await transaction.RollbackAsync(ct);
            return [];
        }

        var now = DateTimeOffset.UtcNow;
        var ticket = KitchenTicket.Create(
            envelope.Payload.OrderId,
            brandId,
            envelope.CorrelationId,
            envelope.Payload.SourceChannel,
            now);

        foreach (var item in envelope.Payload.Items.OrderBy(item => item.OrderItemId))
        {
            if (routes.TryGetValue(item.MenuItemId, out var route)
                && stations.TryGetValue(route.StationId, out var station)
                && station.IsActive)
            {
                ticket.AddTask(KitchenTask.Create(
                    item.OrderItemId,
                    item.MenuItemId,
                    item.Name,
                    item.Quantity,
                    station.Id,
                    station.Code,
                    now));
            }
            else
            {
                ticket.AddTask(KitchenTask.CreateRoutingMissing(
                    item.OrderItemId,
                    item.MenuItemId,
                    item.Name,
                    item.Quantity,
                    now));
            }
        }

        ticket.RefreshStatus(now);
        db.KitchenTickets.Add(ticket);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return ticket.Tasks
            .Where(task => task.Status == KitchenTaskStatus.Pending && task.StationId is not null)
            .OrderBy(task => task.CreatedAt)
            .Select(KitchenTaskResponse.FromTask)
            .ToArray();
    }
}
