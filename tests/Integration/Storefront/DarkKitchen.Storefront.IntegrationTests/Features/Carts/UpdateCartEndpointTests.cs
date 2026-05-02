namespace DarkKitchen.Storefront.IntegrationTests.Features.Carts;

[Collection(AspireAppCollection.Name)]
public sealed class UpdateCartEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task ReplacesCartItemsFromActiveMenu()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var cart = await client.CreateCartAsync();

        var updated = await client.UpdateCartAsync(cart.CartId, StorefrontApiClient.DemoMenuItemGuid, 2);

        var item = Assert.Single(updated.Items);
        Assert.Equal("Classic Smash", item.Name);
        Assert.Equal(65.80m, updated.TotalPrice);
    }

    [Fact]
    public async Task UnavailableMenuItem_ReturnsValidationProblem()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var cart = await client.CreateCartAsync();

        using var response = await client.UpdateCartResponseAsync(cart.CartId, Guid.NewGuid(), 1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
