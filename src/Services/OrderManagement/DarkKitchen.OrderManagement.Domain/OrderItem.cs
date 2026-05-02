namespace DarkKitchen.OrderManagement.Domain;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = "PLN";
    public decimal LineTotal => UnitPrice * Quantity;

    public static OrderItem Create(Guid menuItemId, string name, int quantity, decimal unitPrice, string currency)
    {
        if (menuItemId == Guid.Empty)
        {
            throw new ArgumentException("Menu item id is required.", nameof(menuItemId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), unitPrice, "Unit price cannot be negative.");
        }

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            MenuItemId = menuItemId,
            Name = RequireNonWhiteSpace(name, nameof(name)),
            Quantity = quantity,
            UnitPrice = unitPrice,
            Currency = RequireNonWhiteSpace(currency, nameof(currency)).ToUpperInvariant()
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

    private static string RequireNonWhiteSpace(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }
}
