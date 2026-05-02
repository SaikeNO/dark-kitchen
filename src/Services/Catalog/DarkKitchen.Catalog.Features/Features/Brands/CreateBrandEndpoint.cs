using DarkKitchen.Catalog.Features.Features;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Brands;

public static class CreateBrandEndpoint
{
    public static async Task<IResult> HandleAsync(
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return validation;
        }

        var brand = Brand.Create(
            request.Name.Trim(),
            ApiValidation.TrimOptional(request.Description),
            ApiValidation.TrimOptional(request.LogoUrl),
            request.IsActive,
            DateTimeOffset.UtcNow,
            domains: NormalizeDomains(request.Domains),
            heroTitle: ApiValidation.TrimOptional(request.HeroTitle),
            heroSubtitle: ApiValidation.TrimOptional(request.HeroSubtitle),
            primaryColor: ApiValidation.TrimOptional(request.PrimaryColor),
            accentColor: ApiValidation.TrimOptional(request.AccentColor),
            backgroundColor: ApiValidation.TrimOptional(request.BackgroundColor),
            textColor: ApiValidation.TrimOptional(request.TextColor));

        outbox.DbContext.Brands.Add(brand);
        await outbox.PublishAsync(CatalogEventFactory.BrandChanged(brand, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Created($"/api/admin/brands/{brand.Id}", Response.FromBrand(brand));
    }

    private static IResult? Validate(Request request)
    {
        return string.IsNullOrWhiteSpace(request.Name)
            ? ApiValidation.Problem(("name", "Brand name is required."))
            : null;
    }

    public sealed record Request(
        string Name,
        string? Description,
        string? LogoUrl,
        IReadOnlyList<string>? Domains,
        string? HeroTitle,
        string? HeroSubtitle,
        string? PrimaryColor,
        string? AccentColor,
        string? BackgroundColor,
        string? TextColor,
        bool IsActive);

    public sealed record Response(
        Guid Id,
        string Name,
        string? Description,
        string? LogoUrl,
        IReadOnlyList<string> Domains,
        string? HeroTitle,
        string? HeroSubtitle,
        string PrimaryColor,
        string AccentColor,
        string BackgroundColor,
        string TextColor,
        bool IsActive)
    {
        public static Response FromBrand(Brand brand)
        {
            return new Response(
                brand.Id,
                brand.Name,
                brand.Description,
                brand.LogoUrl,
                brand.Domains,
                brand.HeroTitle,
                brand.HeroSubtitle,
                brand.PrimaryColor,
                brand.AccentColor,
                brand.BackgroundColor,
                brand.TextColor,
                brand.IsActive);
        }
    }

    private static IReadOnlyList<string> NormalizeDomains(IReadOnlyList<string>? domains)
    {
        return domains is null
            ? []
            : domains.Select(domain => domain.Trim()).Where(domain => domain.Length > 0).ToArray();
    }
}
