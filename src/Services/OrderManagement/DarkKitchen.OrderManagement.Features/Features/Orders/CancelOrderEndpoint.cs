using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Orders;

public static class CancelOrderEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid orderId,
        Request request,
        IDbContextOutbox<OrderManagementDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var db = outbox.DbContext;
        var order = await db.Orders.FirstOrDefaultAsync(order => order.Id == orderId, ct);
        if (order is null)
        {
            return Results.NotFound();
        }

        var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Cancelled" : request.Reason.Trim();
        var correlationId = GetCorrelationId(httpContext);
        var changed = order.Cancel(reason, correlationId, DateTimeOffset.UtcNow);
        if (!changed)
        {
            return Results.Problem(
                detail: "Order is already terminal.",
                statusCode: StatusCodes.Status409Conflict);
        }

        await outbox.PublishAsync(OrderManagementEventFactory.OrderCancelled(order, reason, correlationId));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(OrderSummaryResponse.FromOrder(order));
    }

    private static Guid GetCorrelationId(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var header)
            && Guid.TryParse(header.FirstOrDefault(), out var correlationId)
            && correlationId != Guid.Empty)
        {
            return correlationId;
        }

        return Guid.NewGuid();
    }

    public sealed record Request(string? Reason);
}
