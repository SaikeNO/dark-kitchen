using DarkKitchen.Contracts.Events;
using DarkKitchen.Inventory.Features.Handlers;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.IntegrationTests.Features.EventHandling;

[Collection(AspireAppCollection.Name)]
public sealed class RecipeChangedHandlerTests(AspireAppFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RecipeChangedUpdatesSnapshotAndWarehouseItem()
    {
        await using var db = await CreateDbContextAsync();
        var productId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var envelope = Envelope(new RecipeChanged(
            productId,
            brandId,
            [new RecipeChangedItem(ingredientId, $"Event Ingredient {NewSuffix()}", "g", 2)]),
            brandId.ToString("D"));

        await RecipeChangedHandler.Handle(envelope, db, CancellationToken.None);

        var recipe = await db.RecipeSnapshots.Include(snapshot => snapshot.Items).SingleAsync(snapshot => snapshot.ProductId == productId);
        var item = Assert.Single(recipe.Items);
        Assert.Equal(ingredientId, item.IngredientId);
        Assert.Equal(2, item.Quantity);

        var warehouseItem = await db.WarehouseItems.SingleAsync(entity => entity.Id == ingredientId);
        Assert.Equal(item.IngredientName, warehouseItem.Name);
    }
}
