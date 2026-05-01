namespace DarkKitchen.Catalog.IntegrationTests.Features.Ingredients;

[Collection(AspireAppCollection.Name)]
public sealed class UpdateIngredientEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanUpdateIngredient()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var ingredient = await catalog.CreateIngredientAsync($"Old Ingredient {suffix}");

        var updated = await catalog.UpdateIngredientAsync(
            ingredient.Id,
            new IngredientRequest($"Updated Ingredient {suffix}", "pcs", false));

        Assert.Equal(ingredient.Id, updated.Id);
        Assert.Equal($"Updated Ingredient {suffix}", updated.Name);
        Assert.Equal("pcs", updated.Unit);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task MissingIngredient_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PutIngredientAsync(Guid.NewGuid(), new IngredientRequest("Missing Ingredient", "g", true));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
