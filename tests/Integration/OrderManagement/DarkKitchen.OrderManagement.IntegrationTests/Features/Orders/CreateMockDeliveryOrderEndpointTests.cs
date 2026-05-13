using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.IntegrationTests.Features.Orders;

[Collection(AspireAppCollection.Name)]
public sealed class CreateMockDeliveryOrderEndpointTests(AspireAppFixture fixture) : OrderManagementIntegrationTestBase(fixture)
{
    [Theory]
    [InlineData("Glovo", "mock-delivery:glovo")]
    [InlineData("Uber", "mock-delivery:uber")]
    [InlineData("Pyszne", "mock-delivery:pyszne")]
    public async Task CreatesOrderWithSupportedMockDeliverySource(string platform, string sourceChannel)
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var menuItem = await SeedMenuItemAsync(db, brandId);

        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());
        var order = await api.CreateMockDeliveryOrderAsync(platform, brandId, menuItem.MenuItemId, $"md-{NewSuffix()}");

        Assert.Equal(sourceChannel, order.SourceChannel);
        Assert.Equal("Placed", order.Status);
    }

    [Fact]
    public async Task DuplicateExternalOrderIdReturnsExistingOrder()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var menuItem = await SeedMenuItemAsync(db, brandId);
        var externalOrderId = $"md-dup-{NewSuffix()}";

        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());
        var first = await api.CreateMockDeliveryOrderAsync("Glovo", brandId, menuItem.MenuItemId, externalOrderId);
        using var duplicateResponse = await api.PostMockDeliveryOrderAsync("Glovo", brandId, menuItem.MenuItemId, externalOrderId);
        var second = await duplicateResponse.ReadJsonAsync<OrderSummaryResponse>();

        Assert.Equal(HttpStatusCode.OK, duplicateResponse.StatusCode);
        Assert.Equal(first.OrderId, second.OrderId);
        Assert.Equal(1, await db.Orders.CountAsync(order =>
            order.BrandId == brandId
            && order.SourceChannel == "mock-delivery:glovo"
            && order.ExternalOrderId == externalOrderId));
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

    [Fact]
    public async Task UnknownPlatformReturnsValidationProblem()
    {
        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());

        using var response = await api.PostMockDeliveryOrderAsync(new
        {
            platform = "unknown",
            brandId = Guid.NewGuid(),
            externalOrderId = $"md-unknown-{NewSuffix()}",
            customer = (object?)null,
            items = new[] { new { menuItemId = Guid.NewGuid(), quantity = 1 } },
            externalStatus = "created"
        });
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("platform", problem.Errors.Keys);
    }

    [Fact]
    public async Task UnsupportedExternalStatusReturnsValidationProblem()
    {
        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());

        using var response = await api.PostMockDeliveryOrderAsync(new
        {
            platform = "glovo",
            brandId = Guid.NewGuid(),
            externalOrderId = $"md-status-{NewSuffix()}",
            customer = (object?)null,
            items = new[] { new { menuItemId = Guid.NewGuid(), quantity = 1 } },
            externalStatus = "cancelled"
        });
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("externalStatus", problem.Errors.Keys);
    }

    [Fact]
    public async Task EmptyItemsReturnValidationProblem()
    {
        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());

        using var response = await api.PostMockDeliveryOrderAsync(new
        {
            platform = "glovo",
            brandId = Guid.NewGuid(),
            externalOrderId = $"md-empty-{NewSuffix()}",
            customer = (object?)null,
            items = Array.Empty<object>(),
            externalStatus = "placed"
        });
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("items", problem.Errors.Keys);
    }
}
