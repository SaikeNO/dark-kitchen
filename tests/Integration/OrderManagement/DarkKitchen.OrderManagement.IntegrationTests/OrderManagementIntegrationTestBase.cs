using DarkKitchen.Contracts.Events;
using DarkKitchen.OrderManagement.Domain;
using DarkKitchen.OrderManagement.Features.Application;
using DarkKitchen.OrderManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.IntegrationTests;

public abstract class OrderManagementIntegrationTestBase(AspireAppFixture fixture)
{
    protected static readonly Guid DemoBrandId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");
    protected static readonly Guid DemoProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006");

    protected async Task<HttpClient> CreateOrderManagementClientAsync()
    {
        await fixture.WaitForHealthyAsync("order-management-api");
        return fixture.CreateHttpClient("order-management-api");
    }

    protected async Task<OrderManagementDbContext> CreateDbContextAsync()
    {
        await fixture.WaitForHealthyAsync("order-management-api");
        var connectionString = await fixture.GetConnectionStringAsync("order-db")
            ?? throw new InvalidOperationException("Missing order-db connection string.");
        var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(OrderManagementDbContextFactory).Assembly.FullName))
            .Options;

        var db = new OrderManagementDbContext(options);
        await db.Database.MigrateAsync();
        return db;
    }

    protected static string NewSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    protected static IntegrationEventEnvelope<TPayload> Envelope<TPayload>(
        TPayload payload,
        Guid? correlationId = null,
        string? brandId = null)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            correlationId: correlationId ?? Guid.NewGuid(),
            causationId: null,
            brandId: brandId ?? DemoBrandId.ToString("D"),
            payload: payload);
    }

    protected static async Task<MenuItemSnapshot> SeedMenuItemAsync(
        OrderManagementDbContext db,
        Guid brandId,
        Guid? menuItemId = null,
        bool isActive = true)
    {
        var id = menuItemId ?? Guid.NewGuid();
        var snapshot = MenuItemSnapshot.Create(
            id,
            brandId,
            Guid.NewGuid(),
            $"Menu Item {NewSuffix()}",
            null,
            12.50m,
            "PLN",
            isActive,
            DateTimeOffset.UtcNow);

        db.MenuItemSnapshots.Add(snapshot);
        await db.SaveChangesAsync();
        return snapshot;
    }

    protected static async Task<Order> SeedOrderAsync(
        OrderManagementDbContext db,
        Guid brandId,
        Guid? menuItemId = null,
        string sourceChannel = "storefront")
    {
        var itemId = menuItemId ?? Guid.NewGuid();
        var order = Order.Create(
            brandId,
            $"ext-{NewSuffix()}",
            sourceChannel,
            Guid.NewGuid(),
            CustomerSnapshot.Create("Test Customer", "500100200", "Leave at counter"),
            [OrderItem.Create(itemId, "Seeded Item", 2, 12.50m, "PLN")],
            DateTimeOffset.UtcNow);

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }
}
