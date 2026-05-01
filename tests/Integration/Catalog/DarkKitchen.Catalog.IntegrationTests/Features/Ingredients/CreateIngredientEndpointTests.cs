namespace DarkKitchen.Catalog.IntegrationTests.Features.Ingredients;

[Collection(AspireAppCollection.Name)]
public sealed class CreateIngredientEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanCreateIngredient()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();

        var ingredient = await catalog.CreateIngredientAsync($"Ingredient {suffix}");

        Assert.NotEqual(Guid.Empty, ingredient.Id);
        Assert.Equal($"Ingredient {suffix}", ingredient.Name);
        Assert.Equal("g", ingredient.Unit);
    }

    [Fact]
    public async Task BlankUnit_ReturnsValidationProblem()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostIngredientAsync(new IngredientRequest("Ingredient", " ", true));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("unit", problem.Errors.Keys);
    }

    [Fact]
    public async Task Operator_ReturnsForbidden()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.PostIngredientAsync(new IngredientRequest($"Forbidden Ingredient {NewSuffix()}", "g", true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
