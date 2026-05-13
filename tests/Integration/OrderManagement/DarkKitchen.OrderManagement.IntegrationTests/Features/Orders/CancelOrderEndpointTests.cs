namespace DarkKitchen.OrderManagement.IntegrationTests.Features.Orders;

[Collection(AspireAppCollection.Name)]
public sealed class CancelOrderEndpointTests(AspireAppFixture fixture) : OrderManagementIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CancelOrderMovesToCancelled()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var order = await SeedOrderAsync(db, brandId);

        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());
        using var response = await api.CancelOrderAsync(order.Id, "customer_request");
        await response.AssertStatusCodeAsync(HttpStatusCode.OK);

        var cancelled = await response.ReadJsonAsync<OrderSummaryResponse>();
        Assert.Equal("Cancelled", cancelled.Status);
    }
}
