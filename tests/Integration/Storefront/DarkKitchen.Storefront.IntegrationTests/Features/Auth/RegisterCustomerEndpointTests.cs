namespace DarkKitchen.Storefront.IntegrationTests.Features.Auth;

[Collection(AspireAppCollection.Name)]
public sealed class RegisterCustomerEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task RegistersAndSignsInCustomer()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var email = $"customer-{Guid.NewGuid():N}@example.test";

        var session = await client.RegisterAsync(email, "Demo123!");

        Assert.Equal(email, session.Email);
        Assert.Equal("Storefront Customer", session.DisplayName);
    }
}
