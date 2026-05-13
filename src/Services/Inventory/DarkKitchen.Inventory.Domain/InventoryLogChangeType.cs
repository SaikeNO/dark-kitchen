namespace DarkKitchen.Inventory.Domain;

public enum InventoryLogChangeType
{
    Delivery = 1,
    Adjustment = 2,
    Reservation = 3,
    ReservationRelease = 4,
    Consumption = 5
}
