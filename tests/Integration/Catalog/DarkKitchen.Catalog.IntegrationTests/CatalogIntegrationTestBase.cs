namespace DarkKitchen.Catalog.IntegrationTests;

public abstract class CatalogIntegrationTestBase(AspireAppFixture fixture)
{
    protected async Task<CatalogApiClient> CreateManagerClientAsync()
    {
        var client = await CreateClientAsync();
        await client.LoginAsManagerAsync();
        return client;
    }

    protected async Task<CatalogApiClient> CreateOperatorClientAsync()
    {
        var client = await CreateClientAsync();
        await client.LoginAsOperatorAsync();
        return client;
    }

    protected async Task<HttpClient> CreateAnonymousHttpClientAsync()
    {
        await fixture.WaitForHealthyAsync("catalog-api");
        return fixture.CreateHttpClient("catalog-api");
    }

    protected static string NewSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    private async Task<CatalogApiClient> CreateClientAsync()
    {
        await fixture.WaitForHealthyAsync("catalog-api");
        return new CatalogApiClient(fixture.CreateHttpClient("catalog-api"));
    }
}
