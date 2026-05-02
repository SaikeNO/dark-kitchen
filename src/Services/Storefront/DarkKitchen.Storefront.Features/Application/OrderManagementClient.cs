using System.Net.Http.Json;
using DarkKitchen.Storefront.Features.Features.Checkout;

namespace DarkKitchen.Storefront.Features.Application;

public sealed class OrderManagementClient(HttpClient httpClient)
{
    public async Task<OrderSubmissionResponse> SubmitStorefrontOrderAsync(
        SubmitOrderRequest request,
        Guid correlationId,
        CancellationToken ct)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/orders/storefront")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Correlation-Id", correlationId.ToString("D"));

        using var response = await httpClient.SendAsync(message, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<OrderSubmissionResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Order Management returned an empty response.");
    }
}

public sealed record SubmitOrderRequest(
    Guid BrandId,
    string ExternalOrderId,
    CheckoutCustomerRequest? Customer,
    IReadOnlyList<SubmitOrderItemRequest> Items);

public sealed record SubmitOrderItemRequest(Guid MenuItemId, int Quantity);

public sealed record OrderSubmissionResponse(
    Guid OrderId,
    Guid BrandId,
    string ExternalOrderId,
    string SourceChannel,
    string Status,
    Guid CorrelationId);
