namespace DarkKitchen.Catalog.Domain;

public sealed class Category
{
    private Category()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BrandId { get; private set; }
    public Brand? Brand { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<Product> Products { get; private set; } = [];

    public static Category Create(
        Guid brandId,
        string name,
        int sortOrder,
        bool isActive,
        DateTimeOffset now,
        Guid? id = null)
    {
        return new Category
        {
            Id = id ?? Guid.NewGuid(),
            BrandId = brandId,
            Name = name,
            SortOrder = sortOrder,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(Guid brandId, string name, int sortOrder, bool isActive, DateTimeOffset now)
    {
        BrandId = brandId;
        Name = name;
        SortOrder = sortOrder;
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAt = now;
    }
}
