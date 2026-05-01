namespace DarkKitchen.Kds.IntegrationTests.Features.ServiceInfo;

[Collection(AspireAppCollection.Name)]
public sealed class ServiceInfoEndpointsTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task Root_ReturnsServiceStatus()
    {
        await fixture.WaitForHealthyAsync("kds-api");

        using var client = fixture.CreateHttpClient("kds-api");
        using var response = await client.GetAsync("/");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("KDS Service", body, StringComparison.Ordinal);
        Assert.Contains("Kitchen", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Info_ReturnsServiceResponsibilities()
    {
        await fixture.WaitForHealthyAsync("kds-api");

        using var client = fixture.CreateHttpClient("kds-api");
        using var response = await client.GetAsync("/api/info");
        var body = await response.ReadBodyAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("KDS Service", body, StringComparison.Ordinal);
        Assert.Contains("Kitchen tickets", body, StringComparison.Ordinal);
    }
}
