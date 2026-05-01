namespace DarkKitchen.Catalog.IntegrationTests.Features.Recipes;

[Collection(AspireAppCollection.Name)]
public sealed class UpsertRecipeEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanUpsertRecipe()
    {
        using var catalog = await CreateManagerClientAsync();
        var scenario = await catalog.CreateProductScenarioAsync(NewSuffix());

        var recipe = await catalog.UpsertRecipeAsync(scenario.Product.Id, scenario.Ingredient.Id, 3);

        Assert.Equal(scenario.Product.Id, recipe.ProductId);
        var item = Assert.Single(recipe.Items);
        Assert.Equal(scenario.Ingredient.Id, item.IngredientId);
        Assert.Equal(3, item.Quantity);
    }

    [Fact]
    public async Task DuplicateIngredients_ReturnValidationProblem()
    {
        using var catalog = await CreateManagerClientAsync();
        var scenario = await catalog.CreateProductScenarioAsync(NewSuffix());

        using var response = await catalog.PutRecipeAsync(
            scenario.Product.Id,
            new RecipeRequest(
            [
                new RecipeItemRequest(scenario.Ingredient.Id, 1),
                new RecipeItemRequest(scenario.Ingredient.Id, 2)
            ]));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("items", problem.Errors.Keys);
    }

    [Fact]
    public async Task Operator_ReturnsForbidden()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.PutRecipeAsync(
            Guid.NewGuid(),
            new RecipeRequest([new RecipeItemRequest(Guid.NewGuid(), 1)]));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
