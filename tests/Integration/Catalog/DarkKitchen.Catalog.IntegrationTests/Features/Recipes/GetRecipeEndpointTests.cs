namespace DarkKitchen.Catalog.IntegrationTests.Features.Recipes;

[Collection(AspireAppCollection.Name)]
public sealed class GetRecipeEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Operator_CanGetProductRecipe()
    {
        using var manager = await CreateManagerClientAsync();
        var scenario = await manager.CreateProductScenarioAsync(NewSuffix());
        await manager.UpsertRecipeAsync(scenario.Product.Id, scenario.Ingredient.Id, 2);

        using var catalog = await CreateOperatorClientAsync();
        var recipe = await catalog.GetRecipeAsync(scenario.Product.Id);

        Assert.Equal(scenario.Product.Id, recipe.ProductId);
        var item = Assert.Single(recipe.Items);
        Assert.Equal(scenario.Ingredient.Id, item.IngredientId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public async Task MissingProduct_ReturnsNotFound()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.GetRecipeResponseAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
