using DarkKitchen.Contracts.Events;
using DarkKitchen.Kds.Features.Application;
using DarkKitchen.Kds.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.IntegrationTests;

public abstract class KdsIntegrationTestBase(AspireAppFixture fixture)
{
    protected async Task<HttpClient> CreateKdsClientAsync()
    {
        await fixture.WaitForHealthyAsync("kds-api");
        var client = fixture.CreateHttpClient("kds-api");
        client.DefaultRequestHeaders.Add("X-DarkKitchen-Role", "Operator");
        return client;
    }

    protected async Task<KdsDbContext> CreateDbContextAsync()
    {
        await fixture.WaitForHealthyAsync("kds-api");
        var connectionString = await fixture.GetConnectionStringAsync("kds-db")
            ?? throw new InvalidOperationException("Missing kds-db connection string.");
        var options = new DbContextOptionsBuilder<KdsDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(KdsDbContextFactory).Assembly.FullName))
            .Options;

        var db = new KdsDbContext(options);
        await db.Database.MigrateAsync();
        return db;
    }

    protected static string NewSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    protected static IntegrationEventEnvelope<TPayload> Envelope<TPayload>(
        TPayload payload,
        string? brandId = null,
        Guid? correlationId = null)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            correlationId: correlationId ?? Guid.NewGuid(),
            causationId: null,
            brandId: brandId ?? Guid.NewGuid().ToString("D"),
            payload: payload);
    }
}
