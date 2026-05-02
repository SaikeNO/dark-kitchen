namespace DarkKitchen.Storefront.IntegrationTests.Features.Carts;

[Collection(AspireAppCollection.Name)]
public sealed class GetCartEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task ReturnsExistingCart()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var created = await client.CreateCartAsync();

        var cart = await client.GetCartAsync(created.CartId);

        Assert.Equal(created.CartId, cart.CartId);
    }

    [Fact]
    public async Task MissingCart_ReturnsNotFound()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));

        using var response = await client.GetCartResponseAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
