using DarkKitchen.Contracts.Events;
using DarkKitchen.Storefront.Domain;
using DarkKitchen.Storefront.Features.Application;
using DarkKitchen.Storefront.Features.Features.Orders;
using DarkKitchen.Storefront.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.IntegrationTests.Features.Orders;

[Collection("Aspire application")]
public sealed class StorefrontOrderStatusTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task CheckoutCreatesCustomerOrderStatus()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var api = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var cart = await api.CreateCartAsync();
        await api.UpdateCartAsync(cart.CartId, StorefrontApiClient.DemoMenuItemGuid, 1);

        var checkout = await api.CheckoutAsync(cart.CartId, "success");
        var order = await api.GetOrderAsync(checkout.OrderId!.Value);

        Assert.Equal("Placed", order.Status);
        Assert.Equal(StorefrontApiClient.DemoBrandGuid, order.BrandId);
    }

    [Fact]
    public async Task LifecycleEventsUpdateCustomerOrderStatusAndPickupCode()
    {
        await using var db = await CreateDbContextAsync();
        var orderId = Guid.NewGuid();
        db.CustomerOrders.Add(CustomerOrder.Create(
            orderId,
            StorefrontApiClient.DemoBrandGuid,
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        await OrderLifecycleHandlers.Handle(Envelope(new OrderAccepted(orderId, "storefront", [])), db, CancellationToken.None);
        await OrderLifecycleHandlers.Handle(Envelope(new OrderReadyForPickup(orderId, "PU-123456")), db, CancellationToken.None);
        await OrderLifecycleHandlers.Handle(Envelope(new OrderCompleted(orderId)), db, CancellationToken.None);

        var saved = await db.CustomerOrders.SingleAsync(order => order.OrderId == orderId);
        Assert.Equal("Completed", saved.Status);
        Assert.Equal("PU-123456", saved.PickupCode);
    }

    private async Task<StorefrontDbContext> CreateDbContextAsync()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        var connectionString = await fixture.GetConnectionStringAsync("storefront-db")
            ?? throw new InvalidOperationException("Missing storefront-db connection string.");
        var options = new DbContextOptionsBuilder<StorefrontDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(StorefrontDbContextFactory).Assembly.FullName))
            .Options;

        var db = new StorefrontDbContext(options);
        await db.Database.MigrateAsync();
        return db;
    }

    private static IntegrationEventEnvelope<TPayload> Envelope<TPayload>(TPayload payload)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            correlationId: Guid.NewGuid(),
            causationId: null,
            brandId: StorefrontApiClient.DemoBrandGuid.ToString("D"),
            payload: payload);
    }
}
