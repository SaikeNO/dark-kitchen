namespace DarkKitchen.Inventory.Domain;

public sealed class RecipeSnapshot
{
    private RecipeSnapshot()
    {
    }

    public Guid ProductId { get; private set; }
    public Guid BrandId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<RecipeSnapshotItem> Items { get; private set; } = [];

    public static RecipeSnapshot Create(Guid productId, Guid brandId, DateTimeOffset now)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (brandId == Guid.Empty)
        {
            throw new ArgumentException("Brand id is required.", nameof(brandId));
        }

        return new RecipeSnapshot
        {
            ProductId = productId,
            BrandId = brandId,
            UpdatedAt = now
        };
    }

    public void ReplaceItems(Guid brandId, IEnumerable<RecipeSnapshotItem> items, DateTimeOffset now)
    {
        if (brandId == Guid.Empty)
        {
            throw new ArgumentException("Brand id is required.", nameof(brandId));
        }

        BrandId = brandId;
        Items = items.ToList();
        UpdatedAt = now;
    }
}
