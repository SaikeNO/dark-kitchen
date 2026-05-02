namespace DarkKitchen.Storefront.Features.Features.Menu;

public static class GetStorefrontContextEndpoint
{
    public static async Task<IResult> HandleAsync(
        StorefrontDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var brand = await BrandResolver.ResolveAsync(httpContext, db, ct);
        return brand is null
            ? Results.NotFound()
            : Results.Ok(StorefrontMenuMapping.ToContext(brand));
    }
}
