namespace DarkKitchen.OrderManagement.Domain;

public enum OrderStatus
{
    Placed = 1,
    Accepted = 2,
    Preparing = 3,
    ReadyForPacking = 4,
    ReadyForPickup = 5,
    Completed = 6,
    Rejected = 7,
    Cancelled = 8
}
