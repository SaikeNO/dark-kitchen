namespace DarkKitchen.OrderManagement.IntegrationTests.Features.Orders;

[Collection(AspireAppCollection.Name)]
public sealed class CreateMockDeliveryOrderEndpointTests(AspireAppFixture fixture) : OrderManagementIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreatesOrderWithMockDeliverySource()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var menuItem = await SeedMenuItemAsync(db, brandId);

        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());
        var order = await api.CreateMockDeliveryOrderAsync("Glovo", brandId, menuItem.MenuItemId, $"md-{NewSuffix()}");

        Assert.Equal("mock-delivery:glovo", order.SourceChannel);
        Assert.Equal("Placed", order.Status);
    }

    [Fact]
    public async Task MissingPlatformReturnsValidationProblem()
    {
        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());

        using var response = await api.PostMockDeliveryOrderAsync(new
        {
            platform = "",
            brandId = Guid.NewGuid(),
            externalOrderId = $"md-invalid-{NewSuffix()}",
            customer = (object?)null,
            items = new[] { new { menuItemId = Guid.NewGuid(), quantity = 1 } }
        });
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("platform", problem.Errors.Keys);
    }
}
