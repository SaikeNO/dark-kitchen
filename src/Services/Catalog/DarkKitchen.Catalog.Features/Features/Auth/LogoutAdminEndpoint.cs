using Microsoft.AspNetCore.Identity;

namespace DarkKitchen.Catalog.Features.Features.Auth;

public static class LogoutAdminEndpoint
{
    public static async Task<IResult> HandleAsync(SignInManager<CatalogUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }

    public sealed record Response(bool SignedOut);
}
