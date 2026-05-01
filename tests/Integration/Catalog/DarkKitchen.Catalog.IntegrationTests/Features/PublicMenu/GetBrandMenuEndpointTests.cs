namespace DarkKitchen.Catalog.IntegrationTests.Features.PublicMenu;

[Collection(AspireAppCollection.Name)]
public sealed class GetBrandMenuEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task PublicMenu_ReturnsOnlyActiveProductsForRequestedBrand()
    {
        using var manager = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var productA = await manager.CreateActivatableProductAsync($"A{suffix}");
        var productB = await manager.CreateActivatableProductAsync($"B{suffix}");
        var activatedA = await manager.ActivateProductAsync(productA.Id);
        var activatedB = await manager.ActivateProductAsync(productB.Id);

        using var client = await CreateAnonymousHttpClientAsync();
        var publicCatalog = new CatalogApiClient(client);
        var menu = await publicCatalog.GetMenuAsync(activatedA.BrandId);

        Assert.Equal(activatedA.BrandId, menu.BrandId);
        var products = menu.Categories.SelectMany(category => category.Products).ToArray();
        Assert.Contains(products, product => product.Id == activatedA.Id);
        Assert.DoesNotContain(products, product => product.Id == activatedB.Id);
    }

    [Fact]
    public async Task MissingBrand_ReturnsNotFound()
    {
        using var client = await CreateAnonymousHttpClientAsync();

        using var response = await client.GetAsync($"/api/menu/brands/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
