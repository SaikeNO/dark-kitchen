namespace DarkKitchen.Catalog.IntegrationTests.Features.Brands;

[Collection(AspireAppCollection.Name)]
public sealed class ListBrandsEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Operator_CanListBrands()
    {
        using var manager = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var created = await manager.CreateBrandAsync($"List Brand {suffix}");

        using var catalog = await CreateOperatorClientAsync();
        var brands = await catalog.ListBrandsAsync();

        Assert.Contains(brands, brand => brand.Id == created.Id);
    }

    [Fact]
    public async Task AnonymousUser_ReturnsUnauthorized()
    {
        using var client = await CreateAnonymousHttpClientAsync();

        using var response = await client.GetAsync("/api/admin/brands");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
