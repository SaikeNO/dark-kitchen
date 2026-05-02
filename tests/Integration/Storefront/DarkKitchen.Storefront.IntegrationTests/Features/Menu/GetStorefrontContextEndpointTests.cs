namespace DarkKitchen.Storefront.IntegrationTests.Features.Menu;

[Collection(AspireAppCollection.Name)]
public sealed class GetStorefrontContextEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task ReturnsResolvedBrandTheme()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));

        var context = await client.GetContextAsync();

        Assert.Equal(StorefrontApiClient.DemoBrandGuid, context.BrandId);
        Assert.Equal("Burger Ghost", context.BrandName);
        Assert.Equal("#dc2626", context.Theme.PrimaryColor);
    }
}
