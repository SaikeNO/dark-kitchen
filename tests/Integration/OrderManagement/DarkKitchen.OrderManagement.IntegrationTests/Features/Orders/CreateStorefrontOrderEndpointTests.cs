using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.IntegrationTests.Features.Orders;

[Collection(AspireAppCollection.Name)]
public sealed class CreateStorefrontOrderEndpointTests(AspireAppFixture fixture) : OrderManagementIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreatesPlacedOrderAndKeepsCorrelationId()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var menuItem = await SeedMenuItemAsync(db, brandId);
        var correlationId = Guid.NewGuid();

        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());
        var order = await api.CreateStorefrontOrderAsync(brandId, menuItem.MenuItemId, $"sf-{NewSuffix()}", correlationId);

        Assert.NotEqual(Guid.Empty, order.OrderId);
        Assert.Equal(brandId, order.BrandId);
        Assert.Equal("storefront", order.SourceChannel);
        Assert.Equal("Placed", order.Status);
        Assert.Equal(correlationId, order.CorrelationId);
    }

    [Fact]
    public async Task DuplicateExternalOrderIdReturnsExistingOrder()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var menuItem = await SeedMenuItemAsync(db, brandId);
        var externalOrderId = $"sf-dup-{NewSuffix()}";

        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());
        var first = await api.CreateStorefrontOrderAsync(brandId, menuItem.MenuItemId, externalOrderId);
        using var duplicateResponse = await api.PostStorefrontOrderAsync(brandId, menuItem.MenuItemId, externalOrderId);
        var second = await duplicateResponse.ReadJsonAsync<OrderSummaryResponse>();

        Assert.Equal(HttpStatusCode.OK, duplicateResponse.StatusCode);
        Assert.Equal(first.OrderId, second.OrderId);
        Assert.Equal(1, await db.Orders.CountAsync(order =>
            order.BrandId == brandId
            && order.SourceChannel == "storefront"
            && order.ExternalOrderId == externalOrderId));
    }

    [Fact]
    public async Task EmptyItemsReturnValidationProblem()
    {
        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());

        using var response = await api.PostStorefrontOrderAsync(new
        {
            brandId = Guid.NewGuid(),
            externalOrderId = $"sf-invalid-{NewSuffix()}",
            customer = (object?)null,
            items = Array.Empty<object>()
        });
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("items", problem.Errors.Keys);
    }

    [Fact]
    public async Task MissingMenuItemReturnsValidationProblem()
    {
        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());

        using var response = await api.PostStorefrontOrderAsync(Guid.NewGuid(), Guid.NewGuid(), $"sf-missing-{NewSuffix()}");
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("items", problem.Errors.Keys);
    }
}
