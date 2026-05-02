using DarkKitchen.Inventory.Features.Features;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Features.InventoryAdmin;

public static class AdjustInventoryItemEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid ingredientId,
        Request request,
        IDbContextOutbox<InventoryDbContext> outbox,
        CancellationToken ct)
    {
        var item = await outbox.DbContext.WarehouseItems.FirstOrDefaultAsync(entity => entity.Id == ingredientId, ct);
        if (item is null)
        {
            return Results.NotFound();
        }

        if (request.OnHandQuantity < item.ReservedQuantity)
        {
            return ApiValidation.Problem(("onHandQuantity", "On-hand quantity cannot be lower than reserved quantity."));
        }

        if (request.MinSafetyLevel is < 0)
        {
            return ApiValidation.Problem(("minSafetyLevel", "Minimum safety level cannot be negative."));
        }

        var previousOnHand = item.OnHandQuantity;
        var now = DateTimeOffset.UtcNow;
        item.Adjust(request.OnHandQuantity, request.MinSafetyLevel, now);
        outbox.DbContext.InventoryLogs.Add(InventoryLog.Create(
            item.Id,
            InventoryLogChangeType.Adjustment,
            item.OnHandQuantity - previousOnHand,
            item.OnHandQuantity,
            item.ReservedQuantity,
            now,
            note: ApiValidation.TrimOptional(request.Note)));

        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(InventoryItemResponse.FromItem(item));
    }

    public sealed record Request(decimal OnHandQuantity, decimal? MinSafetyLevel, string? Note);
}
