namespace DarkKitchen.Storefront.Features.Features.Orders;

public sealed record StorefrontOrderResponse(
    Guid OrderId,
    Guid BrandId,
    string Status,
    string? FailureReason,
    string? PickupCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static StorefrontOrderResponse FromOrder(CustomerOrder order)
    {
        return new StorefrontOrderResponse(
            order.OrderId,
            order.BrandId,
            order.Status,
            order.FailureReason,
            order.PickupCode,
            order.CreatedAt,
            order.UpdatedAt);
    }
}
