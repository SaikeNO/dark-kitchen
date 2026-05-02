using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Inventory.Features.Application;

public sealed class InventoryReservationService(IInventoryOutbox outbox)
{
    public async Task HandleOrderPlacedAsync(
        IntegrationEventEnvelope<OrderPlaced> envelope,
        CancellationToken ct)
    {
        var db = outbox.DbContext;
        var existing = await LoadReservationAsync(envelope.Payload.OrderId, db, ct);
        if (existing is not null)
        {
            await PublishExistingAsync(envelope, existing, ct);
            return;
        }

        var requirements = await BuildRequirementsAsync(envelope.Payload, db, ct);
        if (requirements is null || requirements.Count == 0)
        {
            await CreateFailureAsync(envelope, InventoryReasonCodes.RecipeMissing, ct);
            return;
        }

        var itemIds = requirements.Keys.ToArray();
        var availableItems = await db.WarehouseItems
            .AsNoTracking()
            .Where(item => itemIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, ct);

        if (availableItems.Count != requirements.Count
            || requirements.Any(requirement =>
                !availableItems.TryGetValue(requirement.Key, out var item)
                || item.AvailableQuantity < requirement.Value))
        {
            await CreateFailureAsync(envelope, InventoryReasonCodes.IngredientUnavailable, ct);
            return;
        }

        var now = DateTimeOffset.UtcNow;
        StockReservation? reservationToRepublish = null;
        var updateFailed = false;

        await using (var transaction = await db.Database.BeginTransactionAsync(ct))
        {
            existing = await LoadReservationAsync(envelope.Payload.OrderId, db, ct);
            if (existing is not null)
            {
                reservationToRepublish = existing;
                await transaction.RollbackAsync(ct);
            }
            else
            {
                foreach (var requirement in requirements.OrderBy(requirement => requirement.Key))
                {
                    var updatedRows = await db.WarehouseItems
                        .Where(item => item.Id == requirement.Key
                            && item.OnHandQuantity - item.ReservedQuantity >= requirement.Value)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(item => item.ReservedQuantity, item => item.ReservedQuantity + requirement.Value)
                            .SetProperty(item => item.UpdatedAt, now), ct);

                    if (updatedRows == 0)
                    {
                        updateFailed = true;
                        await transaction.RollbackAsync(ct);
                        break;
                    }
                }

                if (!updateFailed)
                {
                    var updatedItems = await db.WarehouseItems
                        .Where(item => itemIds.Contains(item.Id))
                        .ToDictionaryAsync(item => item.Id, ct);
                    var reservation = StockReservation.Reserve(
                        envelope.Payload.OrderId,
                        requirements.Select(requirement => StockReservationLine.Create(requirement.Key, requirement.Value)),
                        now);

                    db.StockReservations.Add(reservation);
                    foreach (var requirement in requirements)
                    {
                        var item = updatedItems[requirement.Key];
                        db.InventoryLogs.Add(InventoryLog.Create(
                            item.Id,
                            InventoryLogChangeType.Reservation,
                            -requirement.Value,
                            item.OnHandQuantity,
                            item.ReservedQuantity,
                            now,
                            envelope.Payload.OrderId,
                            reservation.Id,
                            "Order reservation"));
                    }

                    await outbox.PublishAsync(InventoryEventFactory.Reserved(envelope, reservation.Id), ct);
                    await outbox.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                    await outbox.FlushAsync(ct);
                }
            }
        }

        if (reservationToRepublish is not null)
        {
            await PublishExistingAsync(envelope, reservationToRepublish, ct);
            return;
        }

        if (updateFailed)
        {
            await CreateFailureAsync(envelope, InventoryReasonCodes.IngredientUnavailable, ct);
        }
    }

    private static async Task<StockReservation?> LoadReservationAsync(
        Guid orderId,
        InventoryDbContext db,
        CancellationToken ct)
    {
        return await db.StockReservations
            .Include(reservation => reservation.Lines)
            .FirstOrDefaultAsync(reservation => reservation.OrderId == orderId, ct);
    }

    private static async Task<Dictionary<Guid, decimal>?> BuildRequirementsAsync(
        OrderPlaced order,
        InventoryDbContext db,
        CancellationToken ct)
    {
        var productIds = order.Items.Select(item => item.MenuItemId).Distinct().ToArray();
        var recipes = await db.RecipeSnapshots
            .AsNoTracking()
            .Include(recipe => recipe.Items)
            .Where(recipe => productIds.Contains(recipe.ProductId))
            .ToDictionaryAsync(recipe => recipe.ProductId, ct);

        var requirements = new Dictionary<Guid, decimal>();
        foreach (var orderLine in order.Items)
        {
            if (!recipes.TryGetValue(orderLine.MenuItemId, out var recipe) || recipe.Items.Count == 0)
            {
                return null;
            }

            foreach (var recipeItem in recipe.Items)
            {
                requirements.TryGetValue(recipeItem.IngredientId, out var currentQuantity);
                requirements[recipeItem.IngredientId] = currentQuantity + (recipeItem.Quantity * orderLine.Quantity);
            }
        }

        return requirements;
    }

    private async Task CreateFailureAsync(
        IntegrationEventEnvelope<OrderPlaced> envelope,
        string reasonCode,
        CancellationToken ct)
    {
        var db = outbox.DbContext;
        var existing = await LoadReservationAsync(envelope.Payload.OrderId, db, ct);
        if (existing is not null)
        {
            await PublishExistingAsync(envelope, existing, ct);
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        existing = await LoadReservationAsync(envelope.Payload.OrderId, db, ct);
        if (existing is not null)
        {
            await transaction.RollbackAsync(ct);
            await PublishExistingAsync(envelope, existing, ct);
            return;
        }

        db.StockReservations.Add(StockReservation.Fail(envelope.Payload.OrderId, reasonCode, DateTimeOffset.UtcNow));
        await outbox.PublishAsync(InventoryEventFactory.Failed(envelope, reasonCode), ct);
        await outbox.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        await outbox.FlushAsync(ct);
    }

    private async Task PublishExistingAsync(
        IntegrationEventEnvelope<OrderPlaced> envelope,
        StockReservation reservation,
        CancellationToken ct)
    {
        if (reservation.Status == StockReservationStatus.Reserved)
        {
            await outbox.PublishAsync(InventoryEventFactory.Reserved(envelope, reservation.Id), ct);
        }
        else
        {
            await outbox.PublishAsync(InventoryEventFactory.Failed(
                envelope,
                reservation.FailureReasonCode ?? InventoryReasonCodes.IngredientUnavailable), ct);
        }

        await outbox.SaveChangesAsync(ct);
        await outbox.FlushAsync(ct);
    }
}
