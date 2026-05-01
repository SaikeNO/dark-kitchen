namespace DarkKitchen.Catalog.IntegrationTests.Features.Brands;

[Collection(AspireAppCollection.Name)]
public sealed class DeactivateBrandEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanDeactivateBrand()
    {
        using var catalog = await CreateManagerClientAsync();
        var brand = await catalog.CreateBrandAsync($"Deactivate Brand {NewSuffix()}");

        var deactivated = await catalog.DeactivateBrandAsync(brand.Id);

        Assert.Equal(brand.Id, deactivated.Id);
        Assert.False(deactivated.IsActive);
    }

    [Fact]
    public async Task MissingBrand_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostDeactivateBrandAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
