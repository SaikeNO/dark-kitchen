namespace DarkKitchen.Inventory.Domain;

public sealed class StockReservationLine
{
    private StockReservationLine()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ReservationId { get; private set; }
    public StockReservation? Reservation { get; private set; }
    public Guid WarehouseItemId { get; private set; }
    public WarehouseItem? WarehouseItem { get; private set; }
    public decimal Quantity { get; private set; }

    public static StockReservationLine Create(Guid warehouseItemId, decimal quantity)
    {
        if (warehouseItemId == Guid.Empty)
        {
            throw new ArgumentException("Warehouse item id is required.", nameof(warehouseItemId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Reservation quantity must be positive.");
        }

        return new StockReservationLine
        {
            Id = Guid.NewGuid(),
            WarehouseItemId = warehouseItemId,
            Quantity = quantity
        };
    }
}
