using DarkKitchen.Contracts.Events;
using DarkKitchen.Kds.Features.Features.Catalog;
using DarkKitchen.Kds.Features.Features.Kitchen;
using DarkKitchen.Kds.Features.Features.Orders;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Kds.IntegrationTests.Features.Kitchen;

[Collection(AspireAppCollection.Name)]
public sealed class KitchenHubTests(AspireAppFixture fixture) : KdsIntegrationTestBase(fixture)
{
    [Fact]
    public async Task TaskChanged_ReachesOnlyClientsInStationGroup()
    {
        await using var db = await CreateDbContextAsync();
        var brandId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stationId = Guid.NewGuid();
        var otherStationId = Guid.NewGuid();
        var stationCode = $"HUB-{NewSuffix()[..6]}";
        var otherStationCode = $"HUB-{NewSuffix()[..6]}";
        await SeedStationAsync(db, stationId, stationCode);
        await SeedStationAsync(db, otherStationId, otherStationCode);
        await ProductStationRoutingChangedHandler.Handle(
            Envelope(new ProductStationRoutingChanged(productId, brandId, stationId, stationCode), brandId.ToString("D")),
            db,
            CancellationToken.None);
        await OrderAcceptedHandler.CreateTicketAsync(
            Envelope(new OrderAccepted(
                Guid.NewGuid(),
                "test",
                [new OrderAcceptedLine(Guid.NewGuid(), productId, "Burger", 1)]), brandId.ToString("D")),
            db,
            CancellationToken.None);
        var taskId = await db.KitchenTasks.Where(task => task.MenuItemId == productId).Select(task => task.Id).SingleAsync();

        using var api = await CreateKdsClientAsync();
        var first = new TaskCompletionSource<KitchenTaskResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<KitchenTaskResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var other = new TaskCompletionSource<KitchenTaskResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var firstConnection = BuildConnection(api.BaseAddress!, first);
        await using var secondConnection = BuildConnection(api.BaseAddress!, second);
        await using var otherConnection = BuildConnection(api.BaseAddress!, other);

        await firstConnection.StartAsync();
        await secondConnection.StartAsync();
        await otherConnection.StartAsync();
        await firstConnection.InvokeAsync("JoinStation", stationId);
        await secondConnection.InvokeAsync("JoinStation", stationId);
        await otherConnection.InvokeAsync("JoinStation", otherStationId);

        using var response = await api.PostAsync($"/api/kitchen/tasks/{taskId}/start", null);
        response.EnsureSuccessStatusCode();

        Assert.Equal("InProgress", (await first.Task.WaitAsync(TimeSpan.FromSeconds(10))).Status);
        Assert.Equal("InProgress", (await second.Task.WaitAsync(TimeSpan.FromSeconds(10))).Status);
        await Task.Delay(500);
        Assert.False(other.Task.IsCompleted);
    }

    private static HubConnection BuildConnection(
        Uri baseAddress,
        TaskCompletionSource<KitchenTaskResponse> taskChanged)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(baseAddress, "/hubs/kitchen"))
            .WithAutomaticReconnect()
            .Build();

        connection.On<KitchenTaskResponse>("taskChanged", taskChanged.SetResult);
        return connection;
    }

    private async Task SeedStationAsync(KdsDbContext db, Guid stationId, string code)
    {
        await StationChangedHandler.Handle(
            Envelope(new StationChanged(stationId, code, code, "#2f7d57", true), "catalog-global"),
            db,
            CancellationToken.None);
    }
}
