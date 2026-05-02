namespace DarkKitchen.Storefront.Features.Features.Carts;

public sealed record CreateCartRequest(Guid? CartId);

public sealed record UpdateCartRequest(IReadOnlyList<CartItemRequest> Items);

public sealed record CartItemRequest(Guid MenuItemId, int Quantity);

public sealed record CartResponse(
    Guid CartId,
    Guid BrandId,
    decimal TotalPrice,
    string Currency,
    IReadOnlyList<CartItemResponse> Items);

public sealed record CartItemResponse(
    Guid MenuItemId,
    string Name,
    string? ImageUrl,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    decimal LineTotal);

public static class CartMapping
{
    public static CartResponse FromCart(Cart cart)
    {
        return new CartResponse(
            cart.Id,
            cart.BrandId,
            cart.TotalPrice,
            cart.Currency,
            cart.Items
                .OrderBy(item => item.Name)
                .Select(item => new CartItemResponse(
                    item.MenuItemId,
                    item.Name,
                    item.ImageUrl,
                    item.Quantity,
                    item.UnitPrice,
                    item.Currency,
                    item.LineTotal))
                .ToArray());
    }
}
