using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Brands;

public static class DeactivateBrandEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid brandId,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var brand = await outbox.DbContext.Brands.FirstOrDefaultAsync(entity => entity.Id == brandId, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        brand.Deactivate(DateTimeOffset.UtcNow);
        await outbox.PublishAsync(CatalogEventFactory.BrandChanged(brand, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(Response.FromBrand(brand));
    }

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
}
