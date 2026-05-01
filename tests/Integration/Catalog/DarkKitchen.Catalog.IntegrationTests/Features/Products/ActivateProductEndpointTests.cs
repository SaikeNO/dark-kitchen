namespace DarkKitchen.Catalog.IntegrationTests.Features.Products;

[Collection(AspireAppCollection.Name)]
public sealed class ActivateProductEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanActivateProductWithRecipeAndStationRoute()
    {
        using var catalog = await CreateManagerClientAsync();
        var product = await catalog.CreateActivatableProductAsync(NewSuffix());

        var activated = await catalog.ActivateProductAsync(product.Id);

        Assert.Equal(product.Id, activated.Id);
        Assert.True(activated.IsActive);
        Assert.NotNull(activated.StationId);
        Assert.Equal(1, activated.RecipeItemCount);
    }

    [Fact]
    public async Task ProductWithoutRecipeAndStationRoute_ReturnsValidationProblem()
    {
        using var catalog = await CreateManagerClientAsync();
        var scenario = await catalog.CreateProductScenarioAsync(NewSuffix());

        using var response = await catalog.PostActivateProductAsync(scenario.Product.Id);
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("recipe", problem.Errors.Keys);
        Assert.Contains("stationId", problem.Errors.Keys);
    }
}
