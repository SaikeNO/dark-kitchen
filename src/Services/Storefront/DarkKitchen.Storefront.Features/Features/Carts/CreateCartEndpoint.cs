namespace DarkKitchen.Storefront.Features.Features.Carts;

public static class CreateCartEndpoint
{
    public static async Task<IResult> HandleAsync(
        CreateCartRequest? request,
        StorefrontDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var brand = await BrandResolver.ResolveAsync(httpContext, db, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        if (request?.CartId is Guid cartId)
        {
            var existing = await CartAccess.FindCartAsync(db, cartId, brand.BrandId, ct);
            if (existing is not null)
            {
                return Results.Ok(CartMapping.FromCart(existing));
            }
        }

        var now = DateTimeOffset.UtcNow;
        var cart = Cart.Create(brand.BrandId, CartAccess.CurrentUserId(httpContext), now);
        db.Carts.Add(cart);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/storefront/carts/{cart.Id}", CartMapping.FromCart(cart));
    }
}
