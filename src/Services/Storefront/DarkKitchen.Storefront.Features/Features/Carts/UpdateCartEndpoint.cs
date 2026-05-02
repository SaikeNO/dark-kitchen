using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Carts;

public static class UpdateCartEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid cartId,
        UpdateCartRequest request,
        StorefrontDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var brand = await BrandResolver.ResolveAsync(httpContext, db, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        var cartExists = await db.Carts
            .AsNoTracking()
            .AnyAsync(cart => cart.Id == cartId && cart.BrandId == brand.BrandId, ct);
        if (!cartExists)
        {
            return Results.NotFound();
        }

        var validation = Validate(request);
        if (validation is not null)
        {
            return validation;
        }

        var requestedItems = request.Items
            .Where(item => item.Quantity > 0)
            .GroupBy(item => item.MenuItemId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

        var menuItemIds = requestedItems.Keys.ToArray();
        var menuItems = await db.MenuItems
            .AsNoTracking()
            .Where(item => menuItemIds.Contains(item.MenuItemId))
            .ToDictionaryAsync(item => item.MenuItemId, ct);

        foreach (var menuItemId in menuItemIds)
        {
            if (!menuItems.TryGetValue(menuItemId, out var menuItem)
                || menuItem.BrandId != brand.BrandId
                || !menuItem.IsActive)
            {
                return ApiValidation.Problem(("items", "Cart contains unavailable menu item."));
            }
        }

        await db.CartItems
            .Where(item => item.CartId == cartId)
            .ExecuteDeleteAsync(ct);

        db.CartItems.AddRange(
            requestedItems.Select(pair =>
            {
                var item = menuItems[pair.Key];
                return CartItem.Create(cartId, item.MenuItemId, item.Name, item.ImageUrl, pair.Value, item.Price, item.Currency);
            }));
        await db.Carts
            .Where(cart => cart.Id == cartId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(cart => cart.UpdatedAt, DateTimeOffset.UtcNow), ct);
        await db.SaveChangesAsync(ct);

        var updated = await CartAccess.FindCartAsync(db, cartId, brand.BrandId, ct)
            ?? throw new InvalidOperationException("Cart disappeared during update.");
        return Results.Ok(CartMapping.FromCart(updated));
    }

    private static IResult? Validate(UpdateCartRequest request)
    {
        if (request.Items.Any(item => item.MenuItemId == Guid.Empty))
        {
            return ApiValidation.Problem(("items", "Menu item id is required."));
        }

        if (request.Items.Any(item => item.Quantity < 0 || item.Quantity > 99))
        {
            return ApiValidation.Problem(("items", "Quantity must be between 0 and 99."));
        }

        return null;
    }
}
