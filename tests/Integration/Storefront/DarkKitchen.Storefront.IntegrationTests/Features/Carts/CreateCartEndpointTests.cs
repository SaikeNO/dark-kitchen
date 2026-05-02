namespace DarkKitchen.Storefront.IntegrationTests.Features.Carts;

[Collection(AspireAppCollection.Name)]
public sealed class CreateCartEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task CreatesCartForResolvedBrand()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));

        var cart = await client.CreateCartAsync();

        Assert.NotEqual(Guid.Empty, cart.CartId);
        Assert.Equal(StorefrontApiClient.DemoBrandGuid, cart.BrandId);
        Assert.Empty(cart.Items);
    }
}
