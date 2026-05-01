namespace DarkKitchen.Catalog.IntegrationTests.Features.ServiceInfo;

[Collection(AspireAppCollection.Name)]
public sealed class ServiceInfoEndpointsTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task Root_ReturnsServiceStatus()
    {
        await fixture.WaitForHealthyAsync("catalog-api");

        using var client = fixture.CreateHttpClient("catalog-api");
        using var response = await client.GetAsync("/");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Catalog & Recipe Service", body, StringComparison.Ordinal);
        Assert.Contains("Catalog", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Info_ReturnsServiceResponsibilities()
    {
        await fixture.WaitForHealthyAsync("catalog-api");

        using var client = fixture.CreateHttpClient("catalog-api");
        using var response = await client.GetAsync("/api/info");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Catalog & Recipe Service", body, StringComparison.Ordinal);
        Assert.Contains("Recipes", body, StringComparison.Ordinal);
    }
}
