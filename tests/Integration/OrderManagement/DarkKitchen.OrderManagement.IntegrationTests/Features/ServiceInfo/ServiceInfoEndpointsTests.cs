namespace DarkKitchen.OrderManagement.IntegrationTests.Features.ServiceInfo;

[Collection(AspireAppCollection.Name)]
public sealed class ServiceInfoEndpointsTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task Root_ReturnsServiceStatus()
    {
        await fixture.WaitForHealthyAsync("order-management-api");

        using var client = fixture.CreateHttpClient("order-management-api");
        using var response = await client.GetAsync("/");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Order Management Service", body, StringComparison.Ordinal);
        Assert.Contains("Orders", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Info_ReturnsServiceResponsibilities()
    {
        await fixture.WaitForHealthyAsync("order-management-api");

        using var client = fixture.CreateHttpClient("order-management-api");
        using var response = await client.GetAsync("/api/info");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Order Management Service", body, StringComparison.Ordinal);
        Assert.Contains("Saga coordination", body, StringComparison.Ordinal);
    }
}
