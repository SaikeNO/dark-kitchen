using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Auth;

public static class RegisterCustomerEndpoint
{
    public static async Task<IResult> HandleAsync(
        RegisterCustomerRequest request,
        UserManager<StorefrontUser> userManager,
        SignInManager<StorefrontUser> signInManager)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiValidation.Problem(("email", "Email is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiValidation.Problem(("password", "Password is required."));
        }

        var normalizedEmail = request.Email.Trim();
        var existing = await userManager.Users.AnyAsync(user => user.Email == normalizedEmail);
        if (existing)
        {
            return ApiValidation.Problem(("email", "Email is already registered."));
        }

        var user = new StorefrontUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            DisplayName = ApiValidation.TrimOptional(request.DisplayName),
            PhoneNumber = ApiValidation.TrimOptional(request.Phone)
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Results.ValidationProblem(result.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
        }

        await signInManager.SignInAsync(user, isPersistent: true);
        return Results.Created("/api/storefront/auth/me", new CustomerSessionResponse(user.Id, user.Email, user.DisplayName, user.PhoneNumber));
    }
}
