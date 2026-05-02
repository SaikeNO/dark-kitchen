namespace DarkKitchen.Inventory.Domain;

public sealed class RecipeSnapshotItem
{
    private RecipeSnapshotItem()
    {
    }

    public Guid ProductId { get; private set; }
    public RecipeSnapshot? Recipe { get; private set; }
    public Guid IngredientId { get; private set; }
    public string IngredientName { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }

    public static RecipeSnapshotItem Create(
        Guid productId,
        Guid ingredientId,
        string ingredientName,
        string unit,
        decimal quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (ingredientId == Guid.Empty)
        {
            throw new ArgumentException("Ingredient id is required.", nameof(ingredientId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Recipe quantity must be positive.");
        }

        return new RecipeSnapshotItem
        {
            ProductId = productId,
            IngredientId = ingredientId,
            IngredientName = RequireNonWhiteSpace(ingredientName, nameof(ingredientName)),
            Unit = RequireNonWhiteSpace(unit, nameof(unit)),
            Quantity = quantity
        };
    }

    private static string RequireNonWhiteSpace(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }
}
