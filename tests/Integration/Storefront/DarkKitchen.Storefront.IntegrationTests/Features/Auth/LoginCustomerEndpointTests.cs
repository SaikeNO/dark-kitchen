namespace DarkKitchen.Storefront.IntegrationTests.Features.Auth;

[Collection(AspireAppCollection.Name)]
public sealed class LoginCustomerEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task LogsInExistingCustomer()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        var email = $"login-{Guid.NewGuid():N}@example.test";

        using (var registerClient = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api")))
        {
            await registerClient.RegisterAsync(email, "Demo123!");
        }

        using var loginClient = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var session = await loginClient.LoginAsync(email, "Demo123!");

        Assert.Equal(email, session.Email);
    }
}
