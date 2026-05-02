using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Orders;

public static class CreateMockDeliveryOrderEndpoint
{
    public static Task<IResult> HandleAsync(
        Request request,
        IDbContextOutbox<OrderManagementDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Platform))
        {
            return Task.FromResult(ApiValidation.Problem(("platform", "Platform is required.")));
        }

        var sourceChannel = $"mock-delivery:{request.Platform.Trim().ToLowerInvariant()}";
        return CreateOrderHandler.HandleAsync(
            new CreateOrderHandler.Command(
                request.BrandId,
                request.ExternalOrderId,
                sourceChannel,
                request.Customer,
                request.Items),
            outbox,
            httpContext,
            ct);
    }

    public sealed record Request(
        string? Platform,
        Guid BrandId,
        string? ExternalOrderId,
        OrderCustomerRequest? Customer,
        IReadOnlyList<OrderItemRequest>? Items);
}
