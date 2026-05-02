namespace DarkKitchen.Inventory.Domain;

public sealed class InventoryLog
{
    private InventoryLog()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WarehouseItemId { get; private set; }
    public WarehouseItem? WarehouseItem { get; private set; }
    public Guid? OrderId { get; private set; }
    public Guid? ReservationId { get; private set; }
    public InventoryLogChangeType ChangeType { get; private set; }
    public decimal Amount { get; private set; }
    public decimal OnHandAfter { get; private set; }
    public decimal ReservedAfter { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static InventoryLog Create(
        Guid warehouseItemId,
        InventoryLogChangeType changeType,
        decimal amount,
        decimal onHandAfter,
        decimal reservedAfter,
        DateTimeOffset now,
        Guid? orderId = null,
        Guid? reservationId = null,
        string? note = null)
    {
        if (warehouseItemId == Guid.Empty)
        {
            throw new ArgumentException("Warehouse item id is required.", nameof(warehouseItemId));
        }

        return new InventoryLog
        {
            Id = Guid.NewGuid(),
            WarehouseItemId = warehouseItemId,
            ChangeType = changeType,
            Amount = amount,
            OnHandAfter = onHandAfter,
            ReservedAfter = reservedAfter,
            CreatedAt = now,
            OrderId = orderId,
            ReservationId = reservationId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        };
    }
}
