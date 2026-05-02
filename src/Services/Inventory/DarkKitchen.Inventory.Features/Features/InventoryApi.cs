using DarkKitchen.Inventory.Features.Features.InventoryAdmin;
using DarkKitchen.Inventory.Features.Features.ServiceInfo;

namespace DarkKitchen.Inventory.Features.Features;

public static class InventoryApi
{
    public static WebApplication MapInventoryEndpoints(this WebApplication app)
    {
        app.MapServiceInfoEndpoints();
        app.MapInventoryAdminEndpoints();

        return app;
    }
}
