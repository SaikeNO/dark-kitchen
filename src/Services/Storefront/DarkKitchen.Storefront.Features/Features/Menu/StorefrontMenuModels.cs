namespace DarkKitchen.Storefront.Features.Features.Menu;

public sealed record StorefrontContextResponse(
    Guid BrandId,
    string BrandName,
    string? Description,
    string? LogoUrl,
    string? HeroTitle,
    string? HeroSubtitle,
    StorefrontThemeResponse Theme);

public sealed record StorefrontThemeResponse(
    string PrimaryColor,
    string AccentColor,
    string BackgroundColor,
    string TextColor);

public sealed record StorefrontMenuResponse(
    StorefrontContextResponse Brand,
    IReadOnlyList<StorefrontCategoryResponse> Categories);

public sealed record StorefrontCategoryResponse(
    Guid Id,
    string Name,
    int SortOrder,
    IReadOnlyList<StorefrontProductResponse> Products);

public sealed record StorefrontProductResponse(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    string? ImageUrl,
    decimal Price,
    string Currency);

public static class StorefrontMenuMapping
{
    public static StorefrontContextResponse ToContext(BrandSiteSnapshot brand)
    {
        return new StorefrontContextResponse(
            brand.BrandId,
            brand.Name,
            brand.Description,
            brand.LogoUrl,
            brand.HeroTitle,
            brand.HeroSubtitle,
            new StorefrontThemeResponse(
                brand.PrimaryColor,
                brand.AccentColor,
                brand.BackgroundColor,
                brand.TextColor));
    }
}
