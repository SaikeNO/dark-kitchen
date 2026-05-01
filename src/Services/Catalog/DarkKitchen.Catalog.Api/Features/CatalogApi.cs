namespace DarkKitchen.Catalog.Api.Features;

public static class CatalogApi
{
    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapBrandEndpoints();
        app.MapCategoryEndpoints();
        app.MapProductEndpoints();
        app.MapRecipeEndpoints();
        app.MapIngredientEndpoints();
        app.MapStationEndpoints();
        app.MapProductStationRouteEndpoints();
        app.MapPublicMenuEndpoints();

        return app;
    }
}
