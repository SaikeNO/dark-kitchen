namespace DarkKitchen.Packing.Features.Features.ServiceInfo;

public static class ServiceInfoEndpoints
{
    public static IEndpointRouteBuilder MapServiceInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            service = "Packing Service",
            boundedContext = "Packing",
            status = "ready"
        }));

        app.MapGet("/api/info", () => Results.Ok(new
        {
            service = "Packing Service",
            responsibilities = new[]
            {
                "Packing manifest",
                "Event aggregation",
                "Courier handoff"
            }
        }));

        return app;
    }
}
