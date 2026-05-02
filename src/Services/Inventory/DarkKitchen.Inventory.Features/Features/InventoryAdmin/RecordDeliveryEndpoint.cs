using DarkKitchen.Inventory.Features.Features;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Features.InventoryAdmin;

public static class RecordDeliveryEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid ingredientId,
        Request request,
        IDbContextOutbox<InventoryDbContext> outbox,
        CancellationToken ct)
    {
        if (request.Quantity <= 0)
        {
            return ApiValidation.Problem(("quantity", "Delivery quantity must be positive."));
        }

        var item = await outbox.DbContext.WarehouseItems.FirstOrDefaultAsync(entity => entity.Id == ingredientId, ct);
        if (item is null)
        {
            return Results.NotFound();
        }

        var now = DateTimeOffset.UtcNow;
        item.ReceiveDelivery(request.Quantity, now);
        outbox.DbContext.InventoryLogs.Add(InventoryLog.Create(
            item.Id,
            InventoryLogChangeType.Delivery,
            request.Quantity,
            item.OnHandQuantity,
            item.ReservedQuantity,
            now,
            note: ApiValidation.TrimOptional(request.Note)));

        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(InventoryItemResponse.FromItem(item));
    }

    public sealed record Request(decimal Quantity, string? Note);
}
