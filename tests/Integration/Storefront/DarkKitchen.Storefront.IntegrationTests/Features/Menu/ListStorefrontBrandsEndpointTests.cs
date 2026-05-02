namespace DarkKitchen.Storefront.IntegrationTests.Features.Menu;

[Collection(AspireAppCollection.Name)]
public sealed class ListStorefrontBrandsEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task ReturnsActiveBrandsForPicker()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));

        var brands = await client.ListBrandsAsync();

        var brand = Assert.Single(brands);
        Assert.Equal(StorefrontApiClient.DemoBrandGuid, brand.BrandId);
        Assert.Equal("Burger Ghost", brand.BrandName);
    }
}
