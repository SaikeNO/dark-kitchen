using Microsoft.AspNetCore.Identity;

namespace DarkKitchen.Storefront.Features.Features.Auth;

public static class GetCurrentCustomerEndpoint
{
    public static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UserManager<StorefrontUser> userManager)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        return user is null
            ? Results.Content("null", "application/json")
            : Results.Ok(new CustomerSessionResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, user.PhoneNumber));
    }
}
