namespace DarkKitchen.Storefront.Features.Features.Carts;

public static class GetCartEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid cartId,
        StorefrontDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var brand = await BrandResolver.ResolveAsync(httpContext, db, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        var cart = await CartAccess.FindCartAsync(db, cartId, brand.BrandId, ct);
        return cart is null
            ? Results.NotFound()
            : Results.Ok(CartMapping.FromCart(cart));
    }
}
