namespace DarkKitchen.OrderManagement.Domain;

public sealed class CustomerSnapshot
{
    private CustomerSnapshot()
    {
    }

    public string? DisplayName { get; private set; }
    public string? Phone { get; private set; }
    public string? DeliveryNote { get; private set; }
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }

    public static CustomerSnapshot? Create(string? displayName, string? phone, string? deliveryNote)
    {
        displayName = TrimOptional(displayName);
        phone = TrimOptional(phone);
        deliveryNote = TrimOptional(deliveryNote);

        if (displayName is null && phone is null && deliveryNote is null)
        {
            return null;
        }

        return new CustomerSnapshot
        {
            DisplayName = displayName,
            Phone = phone,
            DeliveryNote = deliveryNote
        };
    }

    public void AssignToOrder(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        OrderId = orderId;
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
