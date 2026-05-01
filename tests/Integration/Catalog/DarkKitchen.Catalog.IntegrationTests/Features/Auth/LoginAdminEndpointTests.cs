namespace DarkKitchen.Catalog.IntegrationTests.Features.Auth;

[Collection(AspireAppCollection.Name)]
public sealed class LoginAdminEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ValidManagerCredentials_ReturnRolesAndSetCookie()
    {
        using var catalog = await CreateManagerClientAsync();

        var currentUser = await catalog.GetCurrentUserAsync();

        Assert.Equal("manager@darkkitchen.local", currentUser.Email);
        Assert.Contains("Manager", currentUser.Roles);
    }

    [Fact]
    public async Task MissingCredentials_ReturnValidationProblem()
    {
        using var client = await CreateAnonymousHttpClientAsync();
        var catalog = new CatalogApiClient(client);

        using var response = await catalog.PostLoginAsync(new LoginRequest("", ""));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("credentials", problem.Errors.Keys);
    }

    [Fact]
    public async Task InvalidPassword_ReturnUnauthorized()
    {
        using var client = await CreateAnonymousHttpClientAsync();
        var catalog = new CatalogApiClient(client);

        using var response = await catalog.PostLoginAsync(new LoginRequest("manager@darkkitchen.local", "bad-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
