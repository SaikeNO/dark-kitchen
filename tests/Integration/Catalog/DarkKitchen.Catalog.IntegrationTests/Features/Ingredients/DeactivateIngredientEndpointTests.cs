namespace DarkKitchen.Catalog.IntegrationTests.Features.Ingredients;

[Collection(AspireAppCollection.Name)]
public sealed class DeactivateIngredientEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanDeactivateIngredient()
    {
        using var catalog = await CreateManagerClientAsync();
        var ingredient = await catalog.CreateIngredientAsync($"Deactivate Ingredient {NewSuffix()}");

        var deactivated = await catalog.DeactivateIngredientAsync(ingredient.Id);

        Assert.Equal(ingredient.Id, deactivated.Id);
        Assert.False(deactivated.IsActive);
    }

    [Fact]
    public async Task MissingIngredient_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostDeactivateIngredientAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
