using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Orders;

public static class StorefrontOrderRoutes
{
    public static IEndpointRouteBuilder MapStorefrontOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/storefront/orders");

        group.MapGet("/", ListOrdersAsync);
        group.MapGet("/{orderId:guid}", GetOrderAsync);

        return app;
    }

    private static async Task<IResult> ListOrdersAsync(
        HttpContext httpContext,
        StorefrontDbContext db,
        CancellationToken ct)
    {
        var brand = await BrandResolver.ResolveAsync(httpContext, db, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        var orders = await db.CustomerOrders
            .AsNoTracking()
            .Where(order => order.BrandId == brand.BrandId)
            .OrderByDescending(order => order.CreatedAt)
            .Take(50)
            .ToArrayAsync(ct);

        return Results.Ok(orders.Select(StorefrontOrderResponse.FromOrder).ToArray());
    }

    private static async Task<IResult> GetOrderAsync(
        Guid orderId,
        HttpContext httpContext,
        StorefrontDbContext db,
        CancellationToken ct)
    {
        var brand = await BrandResolver.ResolveAsync(httpContext, db, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        var order = await db.CustomerOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.BrandId == brand.BrandId && order.OrderId == orderId, ct);

        return order is null
            ? Results.NotFound()
            : Results.Ok(StorefrontOrderResponse.FromOrder(order));
    }
}
