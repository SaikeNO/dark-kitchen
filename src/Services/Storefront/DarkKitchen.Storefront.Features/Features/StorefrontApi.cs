using DarkKitchen.Storefront.Features.Features.Auth;
using DarkKitchen.Storefront.Features.Features.Carts;
using DarkKitchen.Storefront.Features.Features.Checkout;
using DarkKitchen.Storefront.Features.Features.Menu;
using DarkKitchen.Storefront.Features.Features.ServiceInfo;

namespace DarkKitchen.Storefront.Features.Features;

public static class StorefrontApi
{
    public static WebApplication MapStorefrontEndpoints(this WebApplication app)
    {
        app.MapServiceInfoEndpoints();
        app.MapStorefrontAuthEndpoints();
        app.MapStorefrontMenuEndpoints();
        app.MapStorefrontCartEndpoints();
        app.MapStorefrontCheckoutEndpoints();

        return app;
    }
}
