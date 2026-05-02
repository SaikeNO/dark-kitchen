namespace DarkKitchen.Storefront.Domain;

public sealed class MenuItemSnapshot
{
    private MenuItemSnapshot()
    {
    }

    public Guid MenuItemId { get; private set; }
    public Guid BrandId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "PLN";
    public bool IsActive { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static MenuItemSnapshot Create(
        Guid menuItemId,
        Guid brandId,
        Guid categoryId,
        string name,
        string? description,
        string? imageUrl,
        decimal price,
        string currency,
        bool isActive,
        DateTimeOffset now)
    {
        var snapshot = new MenuItemSnapshot { MenuItemId = menuItemId };
        snapshot.Update(brandId, categoryId, name, description, imageUrl, price, currency, isActive, now);
        return snapshot;
    }

    public void Update(
        Guid brandId,
        Guid categoryId,
        string name,
        string? description,
        string? imageUrl,
        decimal price,
        string currency,
        bool isActive,
        DateTimeOffset now)
    {
        BrandId = brandId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        Price = price;
        Currency = currency;
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void UpdatePrice(decimal price, string currency, DateTimeOffset now)
    {
        Price = price;
        Currency = currency;
        UpdatedAt = now;
    }
}
