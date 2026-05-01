namespace DarkKitchen.Catalog.Features.Features.ServiceInfo;

public static class ServiceInfoEndpoints
{
    public static IEndpointRouteBuilder MapServiceInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            service = "Catalog & Recipe Service",
            boundedContext = "Catalog",
            status = "ready"
        }));

        app.MapGet("/api/info", () => Results.Ok(new
        {
            service = "Catalog & Recipe Service",
            responsibilities = new[]
            {
                "Brands",
                "Menu",
                "Recipes",
                "Kitchen station routing"
            }
        }));

        return app;
    }
}
