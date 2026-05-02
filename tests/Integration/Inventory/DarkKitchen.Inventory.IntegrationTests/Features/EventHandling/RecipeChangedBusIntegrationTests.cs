namespace DarkKitchen.Inventory.IntegrationTests.Features.EventHandling;

[Collection(AspireAppCollection.Name)]
public sealed class RecipeChangedBusIntegrationTests(AspireAppFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CatalogAdminRecipeUpsertPublishesRecipeChangedAndInventoryApiHandlesIt()
    {
        using var catalog = await CreateCatalogManagerClientAsync();
        using var inventory = new InventoryApiClient(await CreateInventoryClientAsync());
        var scenario = await catalog.CreateProductScenarioAsync(NewSuffix());

        await catalog.UpsertRecipeAsync(scenario.Product.Id, scenario.Ingredient.Id, 3);

        var item = await WaitForInventoryItemAsync(inventory, scenario.Ingredient.Id);

        Assert.Equal(scenario.Ingredient.Name, item.Name);
        Assert.Equal("g", item.Unit);
        Assert.Equal(0, item.OnHandQuantity);
    }

    private static async Task<InventoryItemResponse> WaitForInventoryItemAsync(
        InventoryApiClient inventory,
        Guid ingredientId)
    {
        var deadline = DateTimeOffset.UtcNow.Add(AspireAppFixture.DefaultTimeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var items = await inventory.ListItemsAsync();
            var item = items.FirstOrDefault(candidate => candidate.IngredientId == ingredientId);
            if (item is not null)
            {
                return item;
            }

            await Task.Delay(250);
        }

        Assert.Fail($"Inventory item {ingredientId} was not created from RecipeChanged.");
        throw new InvalidOperationException("Unreachable after Assert.Fail.");
    }
}
