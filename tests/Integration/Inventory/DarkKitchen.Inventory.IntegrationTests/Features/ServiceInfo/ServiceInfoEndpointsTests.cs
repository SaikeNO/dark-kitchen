namespace DarkKitchen.Inventory.IntegrationTests.Features.ServiceInfo;

[Collection(AspireAppCollection.Name)]
public sealed class ServiceInfoEndpointsTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task Root_ReturnsServiceStatus()
    {
        await fixture.WaitForHealthyAsync("inventory-api");

        using var client = fixture.CreateHttpClient("inventory-api");
        using var response = await client.GetAsync("/");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Inventory Service", body, StringComparison.Ordinal);
        Assert.Contains("Inventory", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Info_ReturnsServiceResponsibilities()
    {
        await fixture.WaitForHealthyAsync("inventory-api");

        using var client = fixture.CreateHttpClient("inventory-api");
        using var response = await client.GetAsync("/api/info");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Inventory Service", body, StringComparison.Ordinal);
        Assert.Contains("Stock reservation", body, StringComparison.Ordinal);
    }
}
