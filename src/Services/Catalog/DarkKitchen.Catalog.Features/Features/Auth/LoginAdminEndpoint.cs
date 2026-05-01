using Microsoft.AspNetCore.Identity;

namespace DarkKitchen.Catalog.Features.Features.Auth;

public static class LoginAdminEndpoint
{
    public static async Task<IResult> HandleAsync(
        Request request,
        SignInManager<CatalogUser> signInManager,
        UserManager<CatalogUser> userManager)
    {
        var email = ApiValidation.NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiValidation.Problem(("credentials", "Email and password are required."));
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: true, lockoutOnFailure: false);
        return result.Succeeded
            ? Results.Ok(await Response.FromUserAsync(user, userManager))
            : Results.Unauthorized();
    }

    public sealed record Request(string Email, string Password);

    public sealed record Response(string Email, IReadOnlyList<string> Roles)
    {
        public static async Task<Response> FromUserAsync(CatalogUser user, UserManager<CatalogUser> userManager)
        {
            var roles = await userManager.GetRolesAsync(user);
            return new Response(user.Email ?? string.Empty, roles.Order(StringComparer.Ordinal).ToArray());
        }
    }
}
