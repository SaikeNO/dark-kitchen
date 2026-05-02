using Microsoft.AspNetCore.Identity;

namespace DarkKitchen.Storefront.Features.Features.Auth;

public static class LoginCustomerEndpoint
{
    public static async Task<IResult> HandleAsync(
        LoginCustomerRequest request,
        UserManager<StorefrontUser> userManager,
        SignInManager<StorefrontUser> signInManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: true, lockoutOnFailure: false);
        return result.Succeeded
            ? Results.Ok(new CustomerSessionResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, user.PhoneNumber))
            : Results.Unauthorized();
    }
}
