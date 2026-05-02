namespace DarkKitchen.Storefront.IntegrationTests.Features.Menu;

[Collection(AspireAppCollection.Name)]
public sealed class GetStorefrontMenuEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task ReturnsActiveMenuForResolvedBrand()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));

        var menu = await client.GetMenuAsync();

        Assert.Equal(StorefrontApiClient.DemoBrandGuid, menu.Brand.BrandId);
        var product = Assert.Single(Assert.Single(menu.Categories).Products);
        Assert.Equal(StorefrontApiClient.DemoMenuItemGuid, product.Id);
        Assert.Equal(32.90m, product.Price);
    }
}
