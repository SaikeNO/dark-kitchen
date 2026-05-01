namespace DarkKitchen.Catalog.Domain;

public sealed class RecipeItem
{
    private RecipeItem()
    {
    }

    public Guid ProductId { get; private set; }
    public Recipe? Recipe { get; private set; }
    public Guid IngredientId { get; private set; }
    public Ingredient? Ingredient { get; private set; }
    public decimal Quantity { get; private set; }

    public static RecipeItem Create(Guid productId, Guid ingredientId, decimal quantity)
    {
        return new RecipeItem
        {
            ProductId = productId,
            IngredientId = ingredientId,
            Quantity = quantity
        };
    }
}
