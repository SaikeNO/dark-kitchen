using Microsoft.AspNetCore.Identity;

namespace DarkKitchen.Storefront.Features.Features.Auth;

public static class LogoutCustomerEndpoint
{
    public static async Task<IResult> HandleAsync(SignInManager<StorefrontUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }
}
