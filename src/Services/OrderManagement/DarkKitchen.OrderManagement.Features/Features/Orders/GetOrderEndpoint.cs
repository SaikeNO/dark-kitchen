using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Orders;

public static class GetOrderEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid orderId,
        OrderManagementDbContext db,
        CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(entity => entity.Customer)
            .Include(entity => entity.Items)
            .Include(entity => entity.History)
            .FirstOrDefaultAsync(entity => entity.Id == orderId, ct);

        return order is null
            ? Results.NotFound()
            : Results.Ok(OrderDetailsResponse.FromOrder(order));
    }
}
