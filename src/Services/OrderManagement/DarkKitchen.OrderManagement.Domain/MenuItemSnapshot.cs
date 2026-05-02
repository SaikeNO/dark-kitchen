namespace DarkKitchen.OrderManagement.Domain;

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
        decimal price,
        string currency,
        bool isActive,
        DateTimeOffset now)
    {
        return new MenuItemSnapshot
        {
            MenuItemId = RequireId(menuItemId, nameof(menuItemId)),
            BrandId = RequireId(brandId, nameof(brandId)),
            CategoryId = RequireId(categoryId, nameof(categoryId)),
            Name = RequireNonWhiteSpace(name, nameof(name)),
            Description = TrimOptional(description),
            Price = price < 0 ? throw new ArgumentOutOfRangeException(nameof(price), price, "Price cannot be negative.") : price,
            Currency = RequireNonWhiteSpace(currency, nameof(currency)).ToUpperInvariant(),
            IsActive = isActive,
            UpdatedAt = now
        };
    }

    public void ApplyCatalogData(
        Guid brandId,
        Guid categoryId,
        string name,
        string? description,
        decimal price,
        string currency,
        bool isActive,
        DateTimeOffset now)
    {
        BrandId = RequireId(brandId, nameof(brandId));
        CategoryId = RequireId(categoryId, nameof(categoryId));
        Name = RequireNonWhiteSpace(name, nameof(name));
        Description = TrimOptional(description);
        Price = price < 0 ? throw new ArgumentOutOfRangeException(nameof(price), price, "Price cannot be negative.") : price;
        Currency = RequireNonWhiteSpace(currency, nameof(currency)).ToUpperInvariant();
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void ChangePrice(decimal price, string currency, DateTimeOffset now)
    {
        Price = price < 0 ? throw new ArgumentOutOfRangeException(nameof(price), price, "Price cannot be negative.") : price;
        Currency = RequireNonWhiteSpace(currency, nameof(currency)).ToUpperInvariant();
        UpdatedAt = now;
    }

    private static Guid RequireId(Guid id, string parameterName)
    {
        return id == Guid.Empty
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : id;
    }

    private static string RequireNonWhiteSpace(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
