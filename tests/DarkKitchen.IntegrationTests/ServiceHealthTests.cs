using DarkKitchen.IntegrationTests.Infrastructure;

namespace DarkKitchen.IntegrationTests;

[Collection(AspireAppCollection.Name)]
public sealed class ServiceHealthTests(AspireAppFixture fixture)
{
    [Theory]
    [MemberData(nameof(ApiResources))]
    [Trait("Category", "Integration")]
    public async Task ApiResource_ReturnsHealthAndInfo(string resourceName, string expectedServiceName)
    {
        await fixture.WaitForHealthyAsync(resourceName);

        using var client = fixture.CreateHttpClient(resourceName);
        using var healthResponse = await client.GetAsync("/health");
        using var infoResponse = await client.GetAsync("/api/info");

        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, infoResponse.StatusCode);

        var infoJson = await infoResponse.Content.ReadAsStringAsync();
        Assert.Contains(expectedServiceName, infoJson, StringComparison.Ordinal);
    }

    public static TheoryData<string, string> ApiResources => new()
    {
        { "catalog-api", "Catalog & Recipe Service" },
        { "inventory-api", "Inventory Service" },
        { "order-management-api", "Order Management Service" },
        { "storefront-api", "Storefront Service" },
        { "kds-api", "KDS Service" },
        { "packing-api", "Packing Service" }
    };
}
