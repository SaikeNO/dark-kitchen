using DarkKitchen.Contracts.Events;
using DarkKitchen.Kds.Domain;
using DarkKitchen.Kds.Features.Features.Catalog;
using DarkKitchen.Kds.Features.Features.Orders;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.IntegrationTests.Features.EventHandling;

[Collection(AspireAppCollection.Name)]
public sealed class OrderAcceptedHandlerTests(AspireAppFixture fixture) : KdsIntegrationTestBase(fixture)
{
    [Fact]
    public async Task OrderAccepted_CreatesTicketAndTasksForTwoStations()
    {
        await using var db = await CreateDbContextAsync();
        var scenario = await SeedRouteScenarioAsync(db, stationCount: 2);
        var order = CreateOrder(scenario.BrandId, scenario.ProductIds);

        var pushedTasks = await OrderAcceptedHandler.CreateTicketAsync(order, db, CancellationToken.None);

        var ticket = await db.KitchenTickets
            .AsNoTracking()
            .Include(entity => entity.Tasks)
            .SingleAsync(entity => entity.OrderId == order.Payload.OrderId);
        Assert.Equal(2, ticket.Tasks.Count);
        Assert.All(ticket.Tasks, task => Assert.Equal(KitchenTaskStatus.Pending, task.Status));
        Assert.Equal(2, pushedTasks.Select(task => task.StationId).Distinct().Count());
    }

    [Fact]
    public async Task DuplicateOrderAccepted_DoesNotCreateSecondTicket()
    {
        await using var db = await CreateDbContextAsync();
        var scenario = await SeedRouteScenarioAsync(db, stationCount: 1);
        var order = CreateOrder(scenario.BrandId, scenario.ProductIds);

        await OrderAcceptedHandler.CreateTicketAsync(order, db, CancellationToken.None);
        await OrderAcceptedHandler.CreateTicketAsync(order, db, CancellationToken.None);

        Assert.Equal(1, await db.KitchenTickets.CountAsync(ticket => ticket.OrderId == order.Payload.OrderId));
        Assert.Equal(1, await db.KitchenTasks.CountAsync(task => task.MenuItemId == scenario.ProductIds[0]));
    }

    [Fact]
    public async Task MissingRouting_CreatesRoutingMissingTask()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        await OrderAcceptedHandler.CreateTicketAsync(CreateOrder(brandId, [productId]), db, CancellationToken.None);

        var task = await db.KitchenTasks.AsNoTracking().SingleAsync(entity => entity.MenuItemId == productId);
        Assert.Equal(KitchenTaskStatus.RoutingMissing, task.Status);
        Assert.Null(task.StationId);
    }

    [Fact]
    public async Task RoutingChange_AffectsOnlyNewOrders()
    {
        await using var db = await CreateDbContextAsync();
        var scenario = await SeedRouteScenarioAsync(db, stationCount: 2);
        var productId = scenario.ProductIds[0];
        var firstStationId = scenario.StationIds[0];
        var secondStationId = scenario.StationIds[1];

        var firstOrder = CreateOrder(scenario.BrandId, [productId]);
        await OrderAcceptedHandler.CreateTicketAsync(firstOrder, db, CancellationToken.None);
        await ProductStationRoutingChangedHandler.Handle(
            Envelope(new ProductStationRoutingChanged(productId, scenario.BrandId, secondStationId, "NEXT"), scenario.BrandId.ToString("D")),
            db,
            CancellationToken.None);
        var secondOrder = CreateOrder(scenario.BrandId, [productId]);
        await OrderAcceptedHandler.CreateTicketAsync(secondOrder, db, CancellationToken.None);

        var firstTaskStation = await db.KitchenTasks
            .Where(task => task.Ticket!.OrderId == firstOrder.Payload.OrderId)
            .Select(task => task.StationId)
            .SingleAsync();
        var secondTaskStation = await db.KitchenTasks
            .Where(task => task.Ticket!.OrderId == secondOrder.Payload.OrderId)
            .Select(task => task.StationId)
            .SingleAsync();

        Assert.Equal(firstStationId, firstTaskStation);
        Assert.Equal(secondStationId, secondTaskStation);
    }

    private async Task<RouteScenario> SeedRouteScenarioAsync(KdsDbContext db, int stationCount)
    {
        var brandId = Guid.NewGuid();
        var productIds = Enumerable.Range(0, stationCount).Select(_ => Guid.NewGuid()).ToArray();
        var stationIds = Enumerable.Range(0, stationCount).Select(_ => Guid.NewGuid()).ToArray();

        for (var index = 0; index < stationCount; index++)
        {
            var code = $"S{index}{NewSuffix()[..4]}";
            await StationChangedHandler.Handle(
                Envelope(new StationChanged(stationIds[index], code, $"Station {index}", "#2f7d57", true), "catalog-global"),
                db,
                CancellationToken.None);
            await ProductStationRoutingChangedHandler.Handle(
                Envelope(new ProductStationRoutingChanged(productIds[index], brandId, stationIds[index], code), brandId.ToString("D")),
                db,
                CancellationToken.None);
        }

        return new RouteScenario(brandId, productIds, stationIds);
    }

    private IntegrationEventEnvelope<OrderAccepted> CreateOrder(Guid brandId, IReadOnlyList<Guid> productIds)
    {
        var lines = productIds
            .Select((productId, index) => new OrderAcceptedLine(Guid.NewGuid(), productId, $"Item {index}", index + 1))
            .ToArray();

        return Envelope(new OrderAccepted(Guid.NewGuid(), "test", lines), brandId.ToString("D"));
    }

    private sealed record RouteScenario(Guid BrandId, IReadOnlyList<Guid> ProductIds, IReadOnlyList<Guid> StationIds);
}
