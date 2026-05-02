namespace DarkKitchen.Inventory.Features.Features.InventoryAdmin;

public static class InventoryAdminRoutes
{
    public static IEndpointRouteBuilder MapInventoryAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/inventory");

        group.MapGet("/items", ListInventoryItemsEndpoint.HandleAsync);
        group.MapGet("/shortages", ListShortagesEndpoint.HandleAsync);
        group.MapPost("/items/{ingredientId:guid}/delivery", RecordDeliveryEndpoint.HandleAsync);
        group.MapPost("/items/{ingredientId:guid}/adjustment", AdjustInventoryItemEndpoint.HandleAsync);

        return app;
    }
}
