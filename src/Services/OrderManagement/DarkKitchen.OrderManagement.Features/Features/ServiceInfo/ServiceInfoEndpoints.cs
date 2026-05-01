namespace DarkKitchen.OrderManagement.Features.Features.ServiceInfo;

public static class ServiceInfoEndpoints
{
    public static IEndpointRouteBuilder MapServiceInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            service = "Order Management Service",
            boundedContext = "Orders",
            status = "ready"
        }));

        app.MapGet("/api/info", () => Results.Ok(new
        {
            service = "Order Management Service",
            responsibilities = new[]
            {
                "Order ingestion",
                "Order state machine",
                "Saga coordination"
            }
        }));

        return app;
    }
}
