namespace DarkKitchen.Kds.Features.Features.Kitchen;

public sealed record StationResponse(
    Guid Id,
    string Code,
    string Name,
    string DisplayColor)
{
    public static StationResponse FromStation(KitchenStation station)
    {
        return new StationResponse(station.Id, station.Code, station.Name, station.DisplayColor);
    }
}

public sealed record KitchenTaskResponse(
    Guid Id,
    Guid TicketId,
    Guid OrderId,
    Guid OrderItemId,
    Guid MenuItemId,
    string ItemName,
    int Quantity,
    Guid StationId,
    string StationCode,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt)
{
    public static KitchenTaskResponse FromTask(KitchenTask task)
    {
        if (task.Ticket is null)
        {
            throw new InvalidOperationException("Kitchen task ticket must be loaded before mapping.");
        }

        if (task.StationId is null || string.IsNullOrWhiteSpace(task.StationCode))
        {
            throw new InvalidOperationException("Kitchen task station snapshot is required before mapping.");
        }

        return new KitchenTaskResponse(
            task.Id,
            task.TicketId,
            task.Ticket.OrderId,
            task.OrderItemId,
            task.MenuItemId,
            task.ItemName,
            task.Quantity,
            task.StationId.Value,
            task.StationCode,
            task.Status.ToString(),
            task.CreatedAt,
            task.StartedAt,
            task.CompletedAt);
    }
}
