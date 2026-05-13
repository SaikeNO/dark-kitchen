using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Orders;

public static class CreateMockDeliveryOrderEndpoint
{
    public static Task<IResult> HandleAsync(
        MockDeliveryOrderWebhook request,
        IDeliveryOrderAdapter adapter,
        IDbContextOutbox<OrderManagementDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var mapped = adapter.Map(request);
        if (mapped.Errors.Count > 0 || mapped.Command is null)
        {
            return Task.FromResult(ApiValidation.Problem(mapped.Errors.ToArray()));
        }

        return CreateOrderHandler.HandleAsync(
            mapped.Command,
            outbox,
            httpContext,
            ct);
    }
}
