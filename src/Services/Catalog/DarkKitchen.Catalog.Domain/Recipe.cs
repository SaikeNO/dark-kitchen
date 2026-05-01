namespace DarkKitchen.Catalog.Domain;

public sealed class Recipe
{
    private Recipe()
    {
    }

    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<RecipeItem> Items { get; private set; } = [];

    public static Recipe Create(Guid productId, DateTimeOffset now)
    {
        return new Recipe
        {
            ProductId = productId,
            UpdatedAt = now
        };
    }

    public void ReplaceItems(IEnumerable<RecipeItem> items, DateTimeOffset now)
    {
        Items = items.ToList();
        UpdatedAt = now;
    }
}
