using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Orders;

public static class CreateStorefrontOrderEndpoint
{
    public static Task<IResult> HandleAsync(
        Request request,
        IDbContextOutbox<OrderManagementDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        return CreateOrderHandler.HandleAsync(
            new CreateOrderHandler.Command(
                request.BrandId,
                request.ExternalOrderId,
                "storefront",
                request.Customer,
                request.Items),
            outbox,
            httpContext,
            ct);
    }

    public sealed record Request(
        Guid BrandId,
        string? ExternalOrderId,
        OrderCustomerRequest? Customer,
        IReadOnlyList<OrderItemRequest>? Items);
}
