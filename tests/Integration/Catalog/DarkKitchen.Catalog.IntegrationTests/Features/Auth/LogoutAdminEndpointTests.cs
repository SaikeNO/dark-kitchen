namespace DarkKitchen.Catalog.IntegrationTests.Features.Auth;

[Collection(AspireAppCollection.Name)]
public sealed class LogoutAdminEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AuthenticatedOperator_CanLogout()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.PostLogoutAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousUser_ReturnsUnauthorized()
    {
        using var client = await CreateAnonymousHttpClientAsync();

        using var response = await client.PostAsync("/api/admin/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
