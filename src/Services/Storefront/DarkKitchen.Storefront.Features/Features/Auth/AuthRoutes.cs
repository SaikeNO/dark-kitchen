namespace DarkKitchen.Storefront.Features.Features.Auth;

public static class AuthRoutes
{
    public static IEndpointRouteBuilder MapStorefrontAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/storefront/auth");

        group.MapGet("/me", GetCurrentCustomerEndpoint.HandleAsync);
        group.MapPost("/register", RegisterCustomerEndpoint.HandleAsync);
        group.MapPost("/login", LoginCustomerEndpoint.HandleAsync);
        group.MapPost("/logout", LogoutCustomerEndpoint.HandleAsync).RequireAuthorization();

        return app;
    }
}
