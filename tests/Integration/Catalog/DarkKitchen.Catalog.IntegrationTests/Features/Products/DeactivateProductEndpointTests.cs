namespace DarkKitchen.Catalog.IntegrationTests.Features.Products;

[Collection(AspireAppCollection.Name)]
public sealed class DeactivateProductEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanDeactivateProduct()
    {
        using var catalog = await CreateManagerClientAsync();
        var product = await catalog.CreateActivatableProductAsync(NewSuffix());
        await catalog.ActivateProductAsync(product.Id);

        var deactivated = await catalog.DeactivateProductAsync(product.Id);

        Assert.Equal(product.Id, deactivated.Id);
        Assert.False(deactivated.IsActive);
    }

    [Fact]
    public async Task MissingProduct_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostDeactivateProductAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
