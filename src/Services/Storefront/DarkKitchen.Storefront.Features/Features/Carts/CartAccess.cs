using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Carts;

internal static class CartAccess
{
    public static Guid? CurrentUserId(HttpContext httpContext)
    {
        return Guid.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;
    }

    public static Task<Cart?> FindCartAsync(StorefrontDbContext db, Guid cartId, Guid brandId, CancellationToken ct)
    {
        return db.Carts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.Id == cartId && cart.BrandId == brandId, ct);
    }
}
