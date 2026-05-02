using DarkKitchen.Testing.Http;

namespace DarkKitchen.OrderManagement.IntegrationTests;

public sealed class OrderManagementApiClient(HttpClient httpClient) : IDisposable
{
    public async Task<OrderSummaryResponse> CreateStorefrontOrderAsync(
        Guid brandId,
        Guid menuItemId,
        string externalOrderId,
        Guid? correlationId = null)
    {
        using var response = await PostStorefrontOrderAsync(brandId, menuItemId, externalOrderId, correlationId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<OrderSummaryResponse>();
    }

    public Task<HttpResponseMessage> PostStorefrontOrderAsync(
        Guid brandId,
        Guid menuItemId,
        string externalOrderId,
        Guid? correlationId = null)
    {
        var request = new StorefrontOrderRequest(
            brandId,
            externalOrderId,
            new OrderCustomerRequest("Test Customer", "500100200", "No onions"),
            [new OrderItemRequest(menuItemId, 2)]);

        return SendJsonAsync(HttpMethod.Post, "/api/orders/storefront", request, correlationId);
    }

    public Task<HttpResponseMessage> PostStorefrontOrderAsync(object request)
    {
        return httpClient.PostAsJsonAsync("/api/orders/storefront", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<OrderSummaryResponse> CreateMockDeliveryOrderAsync(
        string platform,
        Guid brandId,
        Guid menuItemId,
        string externalOrderId)
    {
        using var response = await PostMockDeliveryOrderAsync(platform, brandId, menuItemId, externalOrderId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<OrderSummaryResponse>();
    }

    public Task<HttpResponseMessage> PostMockDeliveryOrderAsync(
        string platform,
        Guid brandId,
        Guid menuItemId,
        string externalOrderId)
    {
        var request = new MockDeliveryOrderRequest(
            platform,
            brandId,
            externalOrderId,
            new OrderCustomerRequest("Delivery Customer", "500300400", null),
            [new OrderItemRequest(menuItemId, 1)]);

        return SendJsonAsync(HttpMethod.Post, "/api/mock-delivery/webhooks/orders", request, null);
    }

    public Task<HttpResponseMessage> PostMockDeliveryOrderAsync(object request)
    {
        return httpClient.PostAsJsonAsync("/api/mock-delivery/webhooks/orders", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<OrderDetailsResponse> GetOrderAsync(Guid orderId)
    {
        using var response = await httpClient.GetAsync($"/api/orders/{orderId}");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<OrderDetailsResponse>();
    }

    public Task<HttpResponseMessage> GetOrderResponseAsync(Guid orderId)
    {
        return httpClient.GetAsync($"/api/orders/{orderId}");
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }

    private Task<HttpResponseMessage> SendJsonAsync<T>(HttpMethod method, string path, T body, Guid? correlationId)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(body, options: HttpTestExtensions.JsonOptions)
        };

        if (correlationId is not null)
        {
            request.Headers.Add("X-Correlation-Id", correlationId.Value.ToString("D"));
        }

        return httpClient.SendAsync(request);
    }
}

public sealed record StorefrontOrderRequest(
    Guid BrandId,
    string ExternalOrderId,
    OrderCustomerRequest? Customer,
    IReadOnlyList<OrderItemRequest> Items);

public sealed record MockDeliveryOrderRequest(
    string Platform,
    Guid BrandId,
    string ExternalOrderId,
    OrderCustomerRequest? Customer,
    IReadOnlyList<OrderItemRequest> Items);

public sealed record OrderCustomerRequest(
    string? DisplayName,
    string? Phone,
    string? DeliveryNote);

public sealed record OrderItemRequest(
    Guid MenuItemId,
    int Quantity);

public sealed record OrderSummaryResponse(
    Guid OrderId,
    Guid BrandId,
    string ExternalOrderId,
    string SourceChannel,
    string Status,
    Guid CorrelationId);

public sealed record OrderDetailsResponse(
    Guid OrderId,
    Guid BrandId,
    string ExternalOrderId,
    string SourceChannel,
    string Status,
    Guid CorrelationId,
    decimal TotalPrice,
    string Currency,
    OrderCustomerResponse? Customer,
    IReadOnlyList<OrderItemResponse> Items,
    IReadOnlyList<OrderHistoryResponse> History);

public sealed record OrderCustomerResponse(
    string? DisplayName,
    string? Phone,
    string? DeliveryNote);

public sealed record OrderItemResponse(
    Guid OrderItemId,
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    decimal LineTotal);

public sealed record OrderHistoryResponse(
    string? FromStatus,
    string ToStatus,
    string? Reason,
    Guid CorrelationId,
    DateTimeOffset CreatedAt);
