using System.Net.Http.Json;
using DarkKitchen.Testing.Aspire;

namespace DarkKitchen.AppHost.IntegrationTests;

[Collection(AspireAppCollection.Name)]
public sealed class EndToEndOrderFlowTests(AspireAppFixture fixture)
{
    private static readonly Guid DemoBrandId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");
    private static readonly Guid DemoProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006");

    [Fact]
    [Trait("Category", "Integration")]
    public async Task StorefrontOrderMovesThroughKitchenPackingAndPickup()
    {
        await WaitForApisAsync();

        var order = await CheckoutDemoOrderAsync(quantity: 1);

        await WaitForOrderStatusAsync(order.OrderId!.Value, "Accepted");

        using var kds = fixture.CreateHttpClient("kds-api");
        kds.DefaultRequestHeaders.Add("X-DarkKitchen-Role", "Operator");
        var station = await GetGrillStationAsync(kds);
        var task = await WaitForKitchenTaskAsync(kds, station.Id, order.OrderId.Value);

        using (var started = await kds.PostAsync($"/api/kitchen/tasks/{task.Id}/start", null))
        {
            started.EnsureSuccessStatusCode();
        }

        using (var completed = await kds.PostAsync($"/api/kitchen/tasks/{task.Id}/done", null))
        {
            completed.EnsureSuccessStatusCode();
        }

        using var packing = fixture.CreateHttpClient("packing-api");
        packing.DefaultRequestHeaders.Add("X-DarkKitchen-Role", "Operator");
        var manifest = await WaitForManifestAsync(packing, order.OrderId.Value, "ReadyForPacking");
        using (var issued = await packing.PostAsJsonAsync(
            $"/api/packing/manifests/{manifest.Id}/issued",
            new IssueManifestRequest(manifest.PickupCode)))
        {
            issued.EnsureSuccessStatusCode();
        }

        var completedOrder = await WaitForOrderStatusAsync(order.OrderId.Value, "Completed");
        Assert.Equal(order.CorrelationId, completedOrder.CorrelationId);
        Assert.Contains(completedOrder.History, item => item.CorrelationId == order.CorrelationId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OversizedStorefrontOrderIsRejectedWhenIngredientsAreMissing()
    {
        await WaitForApisAsync();

        var order = await CreateOmsOrderAsync(quantity: 1_000);
        var rejected = await WaitForOrderStatusAsync(order.OrderId, "Rejected");

        Assert.Contains(rejected.History, item => item.Reason == "ingredient_unavailable");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task StorefrontBrandIsolationDoesNotExposeUnknownBrand()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var storefront = fixture.CreateHttpClient("storefront-api");
        var otherBrandId = Guid.NewGuid();

        using var menu = await storefront.GetAsync($"/api/storefront/menu?brandId={otherBrandId:D}");
        using var cart = await storefront.PostAsJsonAsync(
            $"/api/storefront/carts?brandId={otherBrandId:D}",
            new CreateCartRequest(null));

        Assert.Equal(HttpStatusCode.NotFound, menu.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, cart.StatusCode);
    }

    private async Task WaitForApisAsync()
    {
        foreach (var resource in new[]
        {
            "storefront-api",
            "order-management-api",
            "inventory-api",
            "kds-api",
            "packing-api"
        })
        {
            await fixture.WaitForHealthyAsync(resource);
        }
    }

    private async Task<CheckoutResponse> CheckoutDemoOrderAsync(int quantity)
    {
        using var storefront = fixture.CreateHttpClient("storefront-api");
        using var createdCart = await storefront.PostAsJsonAsync(
            $"/api/storefront/carts?brandId={DemoBrandId:D}",
            new CreateCartRequest(null));
        createdCart.EnsureSuccessStatusCode();
        var cart = await createdCart.Content.ReadFromJsonAsync<CartResponse>();

        using var updatedCart = await storefront.PatchAsJsonAsync(
            $"/api/storefront/carts/{cart!.CartId:D}?brandId={DemoBrandId:D}",
            new UpdateCartRequest([new CartItemRequest(DemoProductId, quantity)]));
        updatedCart.EnsureSuccessStatusCode();

        using var checkout = await storefront.PostAsJsonAsync(
            $"/api/storefront/checkout?brandId={DemoBrandId:D}",
            new CheckoutRequest(
                cart.CartId,
                new CheckoutCustomerRequest("Demo Customer", "500600700", "E2E"),
                "success"));
        checkout.EnsureSuccessStatusCode();

        var response = await checkout.Content.ReadFromJsonAsync<CheckoutResponse>();
        Assert.NotNull(response!.OrderId);
        Assert.NotNull(response.CorrelationId);
        return response;
    }

    private async Task<OrderSummaryResponse> CreateOmsOrderAsync(int quantity)
    {
        using var orders = fixture.CreateHttpClient("order-management-api");
        using var response = await orders.PostAsJsonAsync(
            "/api/orders/storefront",
            new StorefrontOrderRequest(
                DemoBrandId,
                $"oversized-{Guid.NewGuid():N}",
                new OrderCustomerRequest("Missing Stock", "500600700", "E2E"),
                [new OrderItemRequest(DemoProductId, quantity)]));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderSummaryResponse>())!;
    }

    private async Task<OrderDetailsResponse> WaitForOrderStatusAsync(Guid orderId, string status)
    {
        using var orders = fixture.CreateHttpClient("order-management-api");
        return await WaitForAsync(async () =>
        {
            using var response = await orders.GetAsync($"/api/orders/{orderId:D}");
            response.EnsureSuccessStatusCode();
            var order = await response.Content.ReadFromJsonAsync<OrderDetailsResponse>();
            return string.Equals(order!.Status, status, StringComparison.Ordinal) ? order : null;
        }, $"order {orderId:D} to reach {status}");
    }

    private static async Task<StationResponse> GetGrillStationAsync(HttpClient kds)
    {
        using var response = await kds.GetAsync("/api/kitchen/stations");
        response.EnsureSuccessStatusCode();
        var stations = await response.Content.ReadFromJsonAsync<IReadOnlyList<StationResponse>>();
        return stations!.Single(station => station.Code == "GRILL");
    }

    private static async Task<KitchenTaskResponse> WaitForKitchenTaskAsync(HttpClient kds, Guid stationId, Guid orderId)
    {
        return await WaitForAsync(async () =>
        {
            using var response = await kds.GetAsync($"/api/kitchen/stations/{stationId:D}/tasks");
            response.EnsureSuccessStatusCode();
            var tasks = await response.Content.ReadFromJsonAsync<IReadOnlyList<KitchenTaskResponse>>();
            return tasks!.SingleOrDefault(task => task.OrderId == orderId);
        }, $"kitchen task for order {orderId:D}");
    }

    private static async Task<PackingManifestResponse> WaitForManifestAsync(HttpClient packing, Guid orderId, string status)
    {
        return await WaitForAsync(async () =>
        {
            using var response = await packing.GetAsync("/api/packing/manifests");
            response.EnsureSuccessStatusCode();
            var manifests = await response.Content.ReadFromJsonAsync<IReadOnlyList<PackingManifestResponse>>();
            return manifests!.SingleOrDefault(manifest =>
                manifest.OrderId == orderId
                && string.Equals(manifest.Status, status, StringComparison.Ordinal));
        }, $"packing manifest for order {orderId:D} to reach {status}");
    }

    private static async Task<T> WaitForAsync<T>(Func<Task<T?>> probe, string description)
        where T : class
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var result = await probe();
            if (result is not null)
            {
                return result;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException($"Timed out waiting for {description}.");
    }

    private sealed record CreateCartRequest(Guid? CartId);
    private sealed record UpdateCartRequest(IReadOnlyList<CartItemRequest> Items);
    private sealed record CartItemRequest(Guid MenuItemId, int Quantity);
    private sealed record CartResponse(Guid CartId);
    private sealed record CheckoutRequest(Guid CartId, CheckoutCustomerRequest? Customer, string? MockPaymentResult);
    private sealed record CheckoutCustomerRequest(string? DisplayName, string? Phone, string? DeliveryNote);
    private sealed record CheckoutResponse(Guid PaymentId, string PaymentStatus, Guid? OrderId, Guid? CorrelationId, string? FailureReason);
    private sealed record StorefrontOrderRequest(Guid BrandId, string ExternalOrderId, OrderCustomerRequest? Customer, IReadOnlyList<OrderItemRequest> Items);
    private sealed record OrderCustomerRequest(string? DisplayName, string? Phone, string? DeliveryNote);
    private sealed record OrderItemRequest(Guid MenuItemId, int Quantity);
    private sealed record OrderSummaryResponse(Guid OrderId, Guid BrandId, string ExternalOrderId, string SourceChannel, string Status, Guid CorrelationId);
    private sealed record StationResponse(Guid Id, string Code, string Name, string DisplayColor);
    private sealed record KitchenTaskResponse(Guid Id, Guid OrderId, string Status);
    private sealed record PackingManifestResponse(Guid Id, Guid OrderId, string Status, string PickupCode);
    private sealed record IssueManifestRequest(string PickupCode);
    private sealed record OrderDetailsResponse(Guid OrderId, string Status, Guid CorrelationId, IReadOnlyList<OrderHistoryResponse> History);
    private sealed record OrderHistoryResponse(string? FromStatus, string ToStatus, string? Reason, Guid CorrelationId, DateTimeOffset CreatedAt);
}
