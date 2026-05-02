namespace DarkKitchen.Storefront.Domain;

public sealed class MenuCategorySnapshot
{
    private MenuCategorySnapshot()
    {
    }

    public Guid CategoryId { get; private set; }
    public Guid BrandId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static MenuCategorySnapshot Create(Guid categoryId, Guid brandId, string name, int sortOrder, bool isActive, DateTimeOffset now)
    {
        var snapshot = new MenuCategorySnapshot { CategoryId = categoryId };
        snapshot.Update(brandId, name, sortOrder, isActive, now);
        return snapshot;
    }

    public void Update(Guid brandId, string name, int sortOrder, bool isActive, DateTimeOffset now)
    {
        BrandId = brandId;
        Name = name;
        SortOrder = sortOrder;
        IsActive = isActive;
        UpdatedAt = now;
    }
}
