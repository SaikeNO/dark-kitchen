namespace DarkKitchen.Storefront.IntegrationTests.Features.Auth;

[Collection(AspireAppCollection.Name)]
public sealed class LogoutCustomerEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task ClearsCustomerSession()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        await client.RegisterAsync($"logout-{Guid.NewGuid():N}@example.test", "Demo123!");

        await client.LogoutAsync();
        var session = await client.GetMeAsync();

        Assert.Null(session);
    }
}
