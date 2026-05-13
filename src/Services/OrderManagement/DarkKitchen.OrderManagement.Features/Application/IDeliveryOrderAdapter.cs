using DarkKitchen.OrderManagement.Features.Features.Orders;

namespace DarkKitchen.OrderManagement.Features.Application;

public interface IDeliveryOrderAdapter
{
    DeliveryOrderAdapterResult Map(MockDeliveryOrderWebhook request);
}

public sealed record MockDeliveryOrderWebhook(
    string? Platform,
    Guid BrandId,
    string? ExternalOrderId,
    OrderCustomerRequest? Customer,
    IReadOnlyList<OrderItemRequest>? Items,
    string? ExternalStatus);

public sealed record DeliveryOrderAdapterResult(
    CreateOrderHandler.Command? Command,
    IReadOnlyList<(string Key, string Error)> Errors);
