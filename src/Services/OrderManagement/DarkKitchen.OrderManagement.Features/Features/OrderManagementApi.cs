using DarkKitchen.OrderManagement.Features.Features.Orders;
using DarkKitchen.OrderManagement.Features.Features.ServiceInfo;

namespace DarkKitchen.OrderManagement.Features.Features;

public static class OrderManagementApi
{
    public static WebApplication MapOrderManagementEndpoints(this WebApplication app)
    {
        app.MapServiceInfoEndpoints();
        app.MapOrderEndpoints();

        return app;
    }
}
