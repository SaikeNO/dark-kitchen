namespace DarkKitchen.Catalog.Domain;

public sealed class Brand
{
    private Brand()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<Category> Categories { get; private set; } = [];
    public List<Product> Products { get; private set; } = [];

    public static Brand Create(
        string name,
        string? description,
        string? logoUrl,
        bool isActive,
        DateTimeOffset now,
        Guid? id = null)
    {
        return new Brand
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            LogoUrl = logoUrl,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string name, string? description, string? logoUrl, bool isActive, DateTimeOffset now)
    {
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAt = now;
    }
}
