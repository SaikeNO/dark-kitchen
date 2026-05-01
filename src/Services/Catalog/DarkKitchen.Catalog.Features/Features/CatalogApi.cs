using DarkKitchen.Catalog.Features.Features.Auth;
using DarkKitchen.Catalog.Features.Features.Brands;
using DarkKitchen.Catalog.Features.Features.Categories;
using DarkKitchen.Catalog.Features.Features.Ingredients;
using DarkKitchen.Catalog.Features.Features.Products;
using DarkKitchen.Catalog.Features.Features.ProductStationRoutes;
using DarkKitchen.Catalog.Features.Features.PublicMenu;
using DarkKitchen.Catalog.Features.Features.Recipes;
using DarkKitchen.Catalog.Features.Features.ServiceInfo;
using DarkKitchen.Catalog.Features.Features.Stations;

namespace DarkKitchen.Catalog.Features.Features;

public static class CatalogApi
{
    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        app.MapServiceInfoEndpoints();
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
