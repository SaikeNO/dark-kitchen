namespace DarkKitchen.Packing.Domain;

public sealed class ManifestItem
{
    private ManifestItem()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ManifestId { get; private set; }
    public PackingManifest? Manifest { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public bool IsReady { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static ManifestItem Create(
        Guid orderItemId,
        Guid menuItemId,
        string itemName,
        int quantity)
    {
        return new ManifestItem
        {
            OrderItemId = orderItemId,
            MenuItemId = menuItemId,
            ItemName = itemName,
            Quantity = quantity
        };
    }

    public bool MarkReady(DateTimeOffset completedAt)
    {
        if (IsReady)
        {
            return false;
        }

        IsReady = true;
        CompletedAt = completedAt;
        return true;
    }

    internal void AssignManifest(PackingManifest manifest)
    {
        Manifest = manifest;
        ManifestId = manifest.Id;
    }
}
