using Microsoft.AspNetCore.SignalR;

namespace DarkKitchen.Packing.Features.Features.Packing;

public static class PackingManifestNotifier
{
    public static Task NotifyManifestChangedAsync(
        IHubContext<PackingHub> hub,
        PackingManifest manifest,
        CancellationToken ct)
    {
        return hub.Clients.All.SendAsync(
            "manifestChanged",
            PackingManifestResponse.FromManifest(manifest, DateTimeOffset.UtcNow, PackingOptions.DelayThreshold),
            ct);
    }
}
