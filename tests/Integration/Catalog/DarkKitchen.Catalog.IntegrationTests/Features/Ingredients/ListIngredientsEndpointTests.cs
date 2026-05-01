namespace DarkKitchen.Catalog.IntegrationTests.Features.Ingredients;

[Collection(AspireAppCollection.Name)]
public sealed class ListIngredientsEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Operator_CanListIngredients()
    {
        using var manager = await CreateManagerClientAsync();
        var created = await manager.CreateIngredientAsync($"List Ingredient {NewSuffix()}");

        using var catalog = await CreateOperatorClientAsync();
        var ingredients = await catalog.ListIngredientsAsync();

        Assert.Contains(ingredients, ingredient => ingredient.Id == created.Id);
    }

    [Fact]
    public async Task AnonymousUser_ReturnsUnauthorized()
    {
        using var client = await CreateAnonymousHttpClientAsync();

        using var response = await client.GetAsync("/api/admin/ingredients");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
