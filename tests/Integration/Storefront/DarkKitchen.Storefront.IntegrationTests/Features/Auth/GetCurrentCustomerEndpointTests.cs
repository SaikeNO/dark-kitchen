namespace DarkKitchen.Storefront.IntegrationTests.Features.Auth;

[Collection(AspireAppCollection.Name)]
public sealed class GetCurrentCustomerEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task ReturnsCurrentSignedInCustomer()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var email = $"me-{Guid.NewGuid():N}@example.test";
        await client.RegisterAsync(email, "Demo123!");

        var session = await client.GetMeAsync();

        Assert.NotNull(session);
        Assert.Equal(email, session.Email);
    }
}
