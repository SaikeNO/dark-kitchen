using DarkKitchen.Catalog.Features.Features;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Brands;

public static class CreateBrandEndpoint
{
    public static async Task<IResult> HandleAsync(
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
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
            DateTimeOffset.UtcNow);

        outbox.DbContext.Brands.Add(brand);
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
        bool IsActive);

    public sealed record Response(
        Guid Id,
        string Name,
        string? Description,
        string? LogoUrl,
        bool IsActive)
    {
        public static Response FromBrand(Brand brand)
        {
            return new Response(brand.Id, brand.Name, brand.Description, brand.LogoUrl, brand.IsActive);
        }
    }
}
