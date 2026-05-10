using Microsoft.AspNetCore.SignalR;

namespace DarkKitchen.Kds.Features.Features.Kitchen;

public sealed class KitchenHub : Hub
{
    public Task JoinStation(Guid stationId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, KitchenHubGroups.Station(stationId));
    }
}

public static class KitchenHubGroups
{
    public static string Station(Guid stationId)
    {
        return $"station:{stationId:D}";
    }
}
