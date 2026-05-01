namespace DarkKitchen.Catalog.IntegrationTests.Features.Auth;

[Collection(AspireAppCollection.Name)]
public sealed class GetCurrentAdminUserEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AuthenticatedOperator_ReturnsCurrentUser()
    {
        using var catalog = await CreateOperatorClientAsync();

        var currentUser = await catalog.GetCurrentUserAsync();

        Assert.Equal("operator@darkkitchen.local", currentUser.Email);
        Assert.Contains("Operator", currentUser.Roles);
    }

    [Fact]
    public async Task AnonymousUser_ReturnsUnauthorized()
    {
        using var client = await CreateAnonymousHttpClientAsync();

        using var response = await client.GetAsync("/api/admin/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
