using Microsoft.AspNetCore.SignalR;

namespace DarkKitchen.Kds.Features.Features.Kitchen;

public static class KitchenTaskNotifier
{
    public static Task NotifyTaskChangedAsync(
        IHubContext<KitchenHub> hub,
        KitchenTaskResponse task,
        CancellationToken ct)
    {
        return hub.Clients
            .Group(KitchenHubGroups.Station(task.StationId))
            .SendAsync("taskChanged", task, ct);
    }
}
