namespace DarkKitchen.Kds.Features.Features.ServiceInfo;

public static class ServiceInfoEndpoints
{
    public static IEndpointRouteBuilder MapServiceInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            service = "KDS Service",
            boundedContext = "Kitchen",
            status = "ready"
        }));

        app.MapGet("/api/info", () => Results.Ok(new
        {
            service = "KDS Service",
            responsibilities = new[]
            {
                "Kitchen tickets",
                "Station task routing",
                "Realtime kitchen display"
            }
        }));

        return app;
    }
}
