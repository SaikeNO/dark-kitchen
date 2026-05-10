using DarkKitchen.Contracts.Events;
using DarkKitchen.Kds.Features.Features.Catalog;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.IntegrationTests.Features.EventHandling;

[Collection(AspireAppCollection.Name)]
public sealed class CatalogReadModelHandlerTests(AspireAppFixture fixture) : KdsIntegrationTestBase(fixture)
{
    [Fact]
    public async Task StationChanged_UpsertsStationReadModel()
    {
        await using var db = await CreateDbContextAsync();
        var stationId = Guid.NewGuid();

        await StationChangedHandler.Handle(
            Envelope(new StationChanged(stationId, $"GR{NewSuffix()[..4]}", "Grill", "#2f7d57", true), "catalog-global"),
            db,
            CancellationToken.None);
        await StationChangedHandler.Handle(
            Envelope(new StationChanged(stationId, $"FR{NewSuffix()[..4]}", "Frytownica", "#dc2626", false), "catalog-global"),
            db,
            CancellationToken.None);

        var station = await db.KitchenStations.AsNoTracking().SingleAsync(entity => entity.Id == stationId);
        Assert.Equal("Frytownica", station.Name);
        Assert.False(station.IsActive);
        Assert.Equal(1, await db.KitchenStations.CountAsync(entity => entity.Id == stationId));
    }

    [Fact]
    public async Task ProductStationRoutingChanged_UpsertsAndClearsRouteSnapshot()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stationId = Guid.NewGuid();

        await ProductStationRoutingChangedHandler.Handle(
            Envelope(new ProductStationRoutingChanged(productId, brandId, stationId, "GRILL"), brandId.ToString("D")),
            db,
            CancellationToken.None);

        var route = await db.ProductStationRoutes.AsNoTracking().SingleAsync(entity => entity.BrandId == brandId && entity.ProductId == productId);
        Assert.Equal(stationId, route.StationId);

        await ProductStationRoutingChangedHandler.Handle(
            Envelope(new ProductStationRoutingChanged(productId, brandId, null, null), brandId.ToString("D")),
            db,
            CancellationToken.None);

        Assert.False(await db.ProductStationRoutes.AnyAsync(entity => entity.BrandId == brandId && entity.ProductId == productId));
    }
}
