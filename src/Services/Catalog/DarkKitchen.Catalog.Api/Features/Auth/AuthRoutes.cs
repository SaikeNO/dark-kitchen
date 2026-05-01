namespace DarkKitchen.Catalog.Api.Features;

public static class AuthRoutes
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/auth/login", LoginAdminEndpoint.HandleAsync).AllowAnonymous();
        app.MapPost("/api/admin/auth/logout", LogoutAdminEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Operator);
        app.MapGet("/api/admin/auth/me", GetCurrentAdminUserEndpoint.HandleAsync).RequireAuthorization(CatalogPolicies.Operator);

        return app;
    }
}
