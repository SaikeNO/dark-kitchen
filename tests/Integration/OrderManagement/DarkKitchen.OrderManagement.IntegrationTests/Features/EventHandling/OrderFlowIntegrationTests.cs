using DarkKitchen.OrderManagement.Domain;
using DarkKitchen.OrderManagement.Features.Application;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.IntegrationTests.Features.EventHandling;

[Collection(AspireAppCollection.Name)]
public sealed class OrderFlowIntegrationTests(AspireAppFixture fixture) : OrderManagementIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DemoMockDeliveryOrderMovesFromPlacedToAcceptedAfterInventoryReserved()
    {
        using var api = new OrderManagementApiClient(await CreateOrderManagementClientAsync());
        var created = await api.CreateMockDeliveryOrderAsync(
            "glovo",
            DemoBrandId,
            DemoProductId,
            $"flow-{NewSuffix()}");

        await using var db = await CreateDbContextAsync();
        var accepted = await WaitForStatusAsync(db, created.OrderId, OrderStatus.Accepted, TimeSpan.FromSeconds(20));

        Assert.True(accepted);
    }

    private static async Task<bool> WaitForStatusAsync(
        OrderManagementDbContext db,
        Guid orderId,
        OrderStatus expected,
        TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            db.ChangeTracker.Clear();
            var status = await db.Orders
                .Where(order => order.Id == orderId)
                .Select(order => order.Status)
                .SingleAsync();

            if (status == expected)
            {
                return true;
            }

            await Task.Delay(250);
        }

        return false;
    }
}
