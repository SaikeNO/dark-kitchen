using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Brands;

public static class UpdateBrandEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid brandId,
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

        var brand = await outbox.DbContext.Brands.FirstOrDefaultAsync(entity => entity.Id == brandId, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        brand.Update(
            request.Name.Trim(),
            ApiValidation.TrimOptional(request.Description),
            ApiValidation.TrimOptional(request.LogoUrl),
            request.IsActive,
            DateTimeOffset.UtcNow,
            NormalizeDomains(request.Domains),
            ApiValidation.TrimOptional(request.HeroTitle),
            ApiValidation.TrimOptional(request.HeroSubtitle),
            ApiValidation.TrimOptional(request.PrimaryColor),
            ApiValidation.TrimOptional(request.AccentColor),
            ApiValidation.TrimOptional(request.BackgroundColor),
            ApiValidation.TrimOptional(request.TextColor));
        await outbox.PublishAsync(CatalogEventFactory.BrandChanged(brand, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(Response.FromBrand(brand));
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
