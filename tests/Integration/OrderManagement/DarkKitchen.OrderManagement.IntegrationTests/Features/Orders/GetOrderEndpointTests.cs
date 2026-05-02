namespace DarkKitchen.OrderManagement.IntegrationTests.Features.Orders;

[Collection(AspireAppCollection.Name)]
public sealed class GetOrderEndpointTests(AspireAppFixture fixture) : OrderManagementIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ReturnsOrderDetailsWithItemsAndHistory()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var order = await SeedOrderAsync(db, brandId);

        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());
        var details = await api.GetOrderAsync(order.Id);

        Assert.Equal(order.Id, details.OrderId);
        Assert.Equal(brandId, details.BrandId);
        Assert.Single(details.Items);
        Assert.Single(details.History);
        Assert.Equal("Placed", details.History[0].ToStatus);
    }

    [Fact]
    public async Task UnknownOrderReturnsNotFound()
    {
        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());

        using var response = await api.GetOrderResponseAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
