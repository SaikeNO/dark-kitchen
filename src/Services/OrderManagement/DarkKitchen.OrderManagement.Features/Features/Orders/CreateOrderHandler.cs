using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.OrderManagement.Features.Features.Orders;

public static class CreateOrderHandler
{
    public sealed record Command(
        Guid BrandId,
        string? ExternalOrderId,
        string SourceChannel,
        OrderCustomerRequest? Customer,
        IReadOnlyList<OrderItemRequest>? Items);

    public static async Task<IResult> HandleAsync(
        Command command,
        IDbContextOutbox<OrderManagementDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var validation = Validate(command);
        if (validation is not null)
        {
            return validation;
        }

        var db = outbox.DbContext;
        var brandId = command.BrandId;
        var externalOrderId = command.ExternalOrderId!.Trim();
        var sourceChannel = command.SourceChannel.Trim();

        var existing = await FindExistingAsync(db, brandId, sourceChannel, externalOrderId, ct);
        if (existing is not null)
        {
            return Results.Ok(OrderSummaryResponse.FromOrder(existing));
        }

        var menuItems = await LoadMenuItemsAsync(db, brandId, command.Items!, ct);
        if (menuItems.Result is not null)
        {
            return menuItems.Result;
        }

        var correlationId = GetCorrelationId(httpContext);
        var now = DateTimeOffset.UtcNow;
        var order = Order.Create(
            brandId,
            externalOrderId,
            sourceChannel,
            correlationId,
            CustomerSnapshot.Create(
                command.Customer?.DisplayName,
                command.Customer?.Phone,
                command.Customer?.DeliveryNote),
            command.Items!.Select(item =>
            {
                var snapshot = menuItems.Snapshots[item.MenuItemId];
                return OrderItem.Create(
                    snapshot.MenuItemId,
                    snapshot.Name,
                    item.Quantity,
                    snapshot.Price,
                    snapshot.Currency);
            }),
            now);

        db.Orders.Add(order);
        await outbox.PublishAsync(OrderManagementEventFactory.OrderPlaced(order));

        try
        {
            await outbox.SaveChangesAndFlushMessagesAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            existing = await FindExistingAsync(db, brandId, sourceChannel, externalOrderId, ct);
            if (existing is not null)
            {
                return Results.Ok(OrderSummaryResponse.FromOrder(existing));
            }

            throw;
        }

        return Results.Created($"/api/orders/{order.Id}", OrderSummaryResponse.FromOrder(order));
    }

    private static IResult? Validate(Command command)
    {
        var errors = new List<(string Key, string Error)>();

        if (command.BrandId == Guid.Empty)
        {
            errors.Add(("brandId", "Brand is required."));
        }

        if (string.IsNullOrWhiteSpace(command.ExternalOrderId))
        {
            errors.Add(("externalOrderId", "External order id is required."));
        }

        if (string.IsNullOrWhiteSpace(command.SourceChannel))
        {
            errors.Add(("sourceChannel", "Source channel is required."));
        }

        if (command.Items is null || command.Items.Count == 0)
        {
            errors.Add(("items", "Order must contain at least one item."));
        }
        else
        {
            for (var index = 0; index < command.Items.Count; index++)
            {
                var item = command.Items[index];
                if (item.MenuItemId == Guid.Empty)
                {
                    errors.Add(($"items[{index}].menuItemId", "Menu item is required."));
                }

                if (item.Quantity <= 0)
                {
                    errors.Add(($"items[{index}].quantity", "Quantity must be positive."));
                }
            }
        }

        return errors.Count == 0 ? null : ApiValidation.Problem(errors.ToArray());
    }

    private static async Task<Order?> FindExistingAsync(
        OrderManagementDbContext db,
        Guid brandId,
        string sourceChannel,
        string externalOrderId,
        CancellationToken ct)
    {
        return await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(order =>
                order.BrandId == brandId
                && order.SourceChannel == sourceChannel
                && order.ExternalOrderId == externalOrderId, ct);
    }

    private static async Task<MenuValidationResult> LoadMenuItemsAsync(
        OrderManagementDbContext db,
        Guid brandId,
        IReadOnlyList<OrderItemRequest> items,
        CancellationToken ct)
    {
        var menuItemIds = items.Select(item => item.MenuItemId).Distinct().ToArray();
        var snapshots = await db.MenuItemSnapshots
            .AsNoTracking()
            .Where(item => menuItemIds.Contains(item.MenuItemId))
            .ToDictionaryAsync(item => item.MenuItemId, ct);

        var errors = new List<(string Key, string Error)>();
        foreach (var item in items)
        {
            if (!snapshots.TryGetValue(item.MenuItemId, out var snapshot))
            {
                errors.Add(("items", $"Menu item '{item.MenuItemId}' is not available."));
                continue;
            }

            if (snapshot.BrandId != brandId)
            {
                errors.Add(("items", $"Menu item '{item.MenuItemId}' does not belong to brand '{brandId}'."));
            }
            else if (!snapshot.IsActive)
            {
                errors.Add(("items", $"Menu item '{item.MenuItemId}' is inactive."));
            }
        }

        return errors.Count == 0
            ? new MenuValidationResult(null, snapshots)
            : new MenuValidationResult(ApiValidation.Problem(errors.ToArray()), snapshots);
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

    private sealed record MenuValidationResult(
        IResult? Result,
        IReadOnlyDictionary<Guid, MenuItemSnapshot> Snapshots);
}
