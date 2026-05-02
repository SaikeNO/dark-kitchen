using DarkKitchen.Contracts.Events;
using DarkKitchen.Inventory.Features.Application;
using DarkKitchen.Inventory.Infrastructure.Persistence;
using DarkKitchen.Testing.Catalog;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.IntegrationTests;

public abstract class InventoryIntegrationTestBase(AspireAppFixture fixture)
{
    protected async Task<HttpClient> CreateInventoryClientAsync()
    {
        await fixture.WaitForHealthyAsync("inventory-api");
        return fixture.CreateHttpClient("inventory-api");
    }

    protected async Task<CatalogApiClient> CreateCatalogManagerClientAsync()
    {
        await fixture.WaitForHealthyAsync("catalog-api");
        var client = new CatalogApiClient(fixture.CreateHttpClient("catalog-api"));
        await client.LoginAsManagerAsync();
        return client;
    }

    protected async Task<InventoryDbContext> CreateDbContextAsync()
    {
        await fixture.WaitForHealthyAsync("inventory-api");
        var connectionString = await fixture.GetConnectionStringAsync("inventory-db")
            ?? throw new InvalidOperationException("Missing inventory-db connection string.");
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(InventoryDbContextFactory).Assembly.FullName))
            .Options;

        var db = new InventoryDbContext(options);
        await db.Database.MigrateAsync();
        return db;
    }

    protected static string NewSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    protected static IntegrationEventEnvelope<TPayload> Envelope<TPayload>(TPayload payload, string brandId = "test-brand")
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            correlationId: Guid.NewGuid(),
            causationId: null,
            brandId: brandId,
            payload: payload);
    }
}
