namespace DarkKitchen.Storefront.Domain;

public sealed class CartItem
{
    private CartItem()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CartId { get; private set; }
    public Cart? Cart { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = "PLN";
    public decimal LineTotal => Quantity * UnitPrice;

    public static CartItem Create(Guid cartId, Guid menuItemId, string name, string? imageUrl, int quantity, decimal unitPrice, string currency)
    {
        return new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            MenuItemId = menuItemId,
            Name = name,
            ImageUrl = imageUrl,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Currency = currency
        };
    }
}
