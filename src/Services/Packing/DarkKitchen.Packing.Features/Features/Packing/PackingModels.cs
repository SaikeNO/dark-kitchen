namespace DarkKitchen.Packing.Features.Features.Packing;

public sealed record PackingManifestResponse(
    Guid Id,
    Guid OrderId,
    Guid BrandId,
    string SourceChannel,
    string Status,
    int TotalItemsCount,
    int ReadyItemsCount,
    bool IsDelayed,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ReadyForPackingAt,
    DateTimeOffset? IssuedAt,
    string PickupCode,
    IReadOnlyList<ManifestItemResponse> Items)
{
    public static PackingManifestResponse FromManifest(
        PackingManifest manifest,
        DateTimeOffset now,
        TimeSpan delayThreshold)
    {
        var isDelayed = manifest.Status == PackingManifestStatus.Waiting
            && now - manifest.CreatedAt >= delayThreshold;

        return new PackingManifestResponse(
            manifest.Id,
            manifest.OrderId,
            manifest.BrandId,
            manifest.SourceChannel,
            isDelayed ? "Delayed" : manifest.Status.ToString(),
            manifest.TotalItemsCount,
            manifest.ReadyItemsCount,
            isDelayed,
            manifest.CreatedAt,
            manifest.UpdatedAt,
            manifest.ReadyForPackingAt,
            manifest.IssuedAt,
            PackingEventFactory.PickupCodeFor(manifest.OrderId),
            manifest.Items
                .OrderBy(item => item.ItemName)
                .ThenBy(item => item.OrderItemId)
                .Select(ManifestItemResponse.FromItem)
                .ToArray());
    }
}

public sealed record IssueManifestRequest(string? PickupCode);

public sealed record ManifestItemResponse(
    Guid Id,
    Guid OrderItemId,
    Guid MenuItemId,
    string ItemName,
    int Quantity,
    bool IsReady,
    DateTimeOffset? CompletedAt)
{
    public static ManifestItemResponse FromItem(ManifestItem item)
    {
        return new ManifestItemResponse(
            item.Id,
            item.OrderItemId,
            item.MenuItemId,
            item.ItemName,
            item.Quantity,
            item.IsReady,
            item.CompletedAt);
    }
}
