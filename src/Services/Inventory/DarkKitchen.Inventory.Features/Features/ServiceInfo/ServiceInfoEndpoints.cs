namespace DarkKitchen.Inventory.Features.Features.ServiceInfo;

public static class ServiceInfoEndpoints
{
    public static IEndpointRouteBuilder MapServiceInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            service = "Inventory Service",
            boundedContext = "Inventory",
            status = "ready"
        }));

        app.MapGet("/api/info", () => Results.Ok(new
        {
            service = "Inventory Service",
            responsibilities = new[]
            {
                "Stock tracking",
                "Recipe read model",
                "Stock reservation"
            }
        }));

        return app;
    }
}
