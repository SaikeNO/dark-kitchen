namespace DarkKitchen.Storefront.IntegrationTests.Features.ServiceInfo;

[Collection(AspireAppCollection.Name)]
public sealed class ServiceInfoEndpointsTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task Root_ReturnsServiceStatus()
    {
        await fixture.WaitForHealthyAsync("storefront-api");

        using var client = fixture.CreateHttpClient("storefront-api");
        using var response = await client.GetAsync("/");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Storefront Service", body, StringComparison.Ordinal);
        Assert.Contains("Direct Sales", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Info_ReturnsServiceResponsibilities()
    {
        await fixture.WaitForHealthyAsync("storefront-api");

        using var client = fixture.CreateHttpClient("storefront-api");
        using var response = await client.GetAsync("/api/info");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Storefront Service", body, StringComparison.Ordinal);
        Assert.Contains("Mock payment checkout", body, StringComparison.Ordinal);
    }
}
