namespace DarkKitchen.Storefront.Features.Features.ServiceInfo;

public static class ServiceInfoEndpoints
{
    public static IEndpointRouteBuilder MapServiceInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            service = "Storefront Service",
            boundedContext = "Direct Sales",
            status = "ready"
        }));

        app.MapGet("/api/info", () => Results.Ok(new
        {
            service = "Storefront Service",
            responsibilities = new[]
            {
                "White-label storefront BFF",
                "Customer identity",
                "Mock payment checkout"
            }
        }));

        return app;
    }
}
