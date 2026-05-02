namespace DarkKitchen.Storefront.Domain;

public sealed class Cart
{
    private Cart()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BrandId { get; private set; }
    public Guid? UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public List<CartItem> Items { get; private set; } = [];

    public decimal TotalPrice => Items.Sum(item => item.LineTotal);
    public string Currency => Items.FirstOrDefault()?.Currency ?? "PLN";

    public static Cart Create(Guid brandId, Guid? userId, DateTimeOffset now)
    {
        return new Cart
        {
            Id = Guid.NewGuid(),
            BrandId = brandId,
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void ReplaceItems(IEnumerable<CartItem> items, DateTimeOffset now)
    {
        Items.Clear();
        Items.AddRange(items.Where(item => item.Quantity > 0));
        UpdatedAt = now;
    }

    public void AssignUser(Guid userId, DateTimeOffset now)
    {
        UserId = userId;
        UpdatedAt = now;
    }
}
