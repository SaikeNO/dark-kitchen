namespace DarkKitchen.Catalog.Domain;

public sealed class Product
{
    private Product()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BrandId { get; private set; }
    public Brand? Brand { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "PLN";
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Recipe? Recipe { get; private set; }
    public ProductStationRoute? StationRoute { get; private set; }

    public static Product Create(
        Guid brandId,
        Guid categoryId,
        string name,
        string? description,
        decimal price,
        string currency,
        DateTimeOffset now,
        Guid? id = null,
        bool isActive = false)
    {
        return new Product
        {
            Id = id ?? Guid.NewGuid(),
            BrandId = brandId,
            CategoryId = categoryId,
            Name = name,
            Description = description,
            Price = price,
            Currency = currency,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateCatalogData(
        Guid brandId,
        Guid categoryId,
        string name,
        string? description,
        decimal price,
        string currency,
        DateTimeOffset now)
    {
        BrandId = brandId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        Currency = currency;
        UpdatedAt = now;
    }

    public void Activate(DateTimeOffset now)
    {
        IsActive = true;
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAt = now;
    }

    public void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }
}
