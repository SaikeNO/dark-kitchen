using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace DarkKitchen.Catalog.Api.Features;

public static class GetCurrentAdminUserEndpoint
{
    public static async Task<IResult> HandleAsync(
        ClaimsPrincipal principal,
        UserManager<CatalogUser> userManager)
    {
        var user = await userManager.GetUserAsync(principal);
        return user is null
            ? Results.Unauthorized()
            : Results.Ok(await Response.FromUserAsync(user, userManager));
    }

    public sealed record Response(string Email, IReadOnlyList<string> Roles)
    {
        public static async Task<Response> FromUserAsync(CatalogUser user, UserManager<CatalogUser> userManager)
        {
            var roles = await userManager.GetRolesAsync(user);
            return new Response(user.Email ?? string.Empty, roles.Order(StringComparer.Ordinal).ToArray());
        }
    }
}
