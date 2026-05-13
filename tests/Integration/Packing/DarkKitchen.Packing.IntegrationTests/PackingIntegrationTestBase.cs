using DarkKitchen.Packing.Features.Application;
using DarkKitchen.Packing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Packing.IntegrationTests;

public abstract class PackingIntegrationTestBase(AspireAppFixture fixture)
{
    protected async Task<PackingDbContext> CreateDbContextAsync()
    {
        await fixture.WaitForHealthyAsync("packing-api");
        var connectionString = await fixture.GetConnectionStringAsync("packing-db")
            ?? throw new InvalidOperationException("Missing packing-db connection string.");
        var options = new DbContextOptionsBuilder<PackingDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(PackingDbContextFactory).Assembly.FullName))
            .Options;

        var db = new PackingDbContext(options);
        await db.Database.MigrateAsync();
        return db;
    }

    protected static IntegrationEventEnvelope<TPayload> Envelope<TPayload>(
        TPayload payload,
        string? brandId = null,
        Guid? correlationId = null,
        DateTimeOffset? occurredAt = null)
    {
        return IntegrationEventEnvelope.Create(
            eventId: Guid.NewGuid(),
            occurredAt: occurredAt ?? DateTimeOffset.UtcNow,
            correlationId: correlationId ?? Guid.NewGuid(),
            causationId: null,
            brandId: brandId ?? Guid.NewGuid().ToString("D"),
            payload: payload);
    }
}
