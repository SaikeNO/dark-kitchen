namespace DarkKitchen.Packing.IntegrationTests.Features.ServiceInfo;

[Collection(AspireAppCollection.Name)]
public sealed class ServiceInfoEndpointsTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task Root_ReturnsServiceStatus()
    {
        await fixture.WaitForHealthyAsync("packing-api");

        using var client = fixture.CreateHttpClient("packing-api");
        using var response = await client.GetAsync("/");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Packing Service", body, StringComparison.Ordinal);
        Assert.Contains("Packing", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Info_ReturnsServiceResponsibilities()
    {
        await fixture.WaitForHealthyAsync("packing-api");

        using var client = fixture.CreateHttpClient("packing-api");
        using var response = await client.GetAsync("/api/info");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Packing Service", body, StringComparison.Ordinal);
        Assert.Contains("Courier handoff", body, StringComparison.Ordinal);
    }
}
