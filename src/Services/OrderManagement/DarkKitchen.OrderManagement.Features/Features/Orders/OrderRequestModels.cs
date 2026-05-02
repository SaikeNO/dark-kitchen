namespace DarkKitchen.OrderManagement.Features.Features.Orders;

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
    Guid CorrelationId)
{
    public static OrderSummaryResponse FromOrder(Order order)
    {
        return new OrderSummaryResponse(
            order.Id,
            order.BrandId,
            order.ExternalOrderId,
            order.SourceChannel,
            order.Status.ToString(),
            order.CorrelationId);
    }
}

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
    IReadOnlyList<OrderHistoryResponse> History)
{
    public static OrderDetailsResponse FromOrder(Order order)
    {
        return new OrderDetailsResponse(
            order.Id,
            order.BrandId,
            order.ExternalOrderId,
            order.SourceChannel,
            order.Status.ToString(),
            order.CorrelationId,
            order.TotalPrice,
            order.Currency,
            order.Customer is null
                ? null
                : new OrderCustomerResponse(order.Customer.DisplayName, order.Customer.Phone, order.Customer.DeliveryNote),
            order.Items
                .OrderBy(item => item.Id)
                .Select(item => new OrderItemResponse(
                    item.Id,
                    item.MenuItemId,
                    item.Name,
                    item.Quantity,
                    item.UnitPrice,
                    item.Currency,
                    item.LineTotal))
                .ToArray(),
            order.History
                .OrderBy(history => history.CreatedAt)
                .Select(history => new OrderHistoryResponse(
                    history.FromStatus?.ToString(),
                    history.ToStatus.ToString(),
                    history.Reason,
                    history.CorrelationId,
                    history.CreatedAt))
                .ToArray());
    }
}

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
