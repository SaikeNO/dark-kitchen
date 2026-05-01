using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class UpdateBrandEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid brandId,
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
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

        brand.Name = request.Name.Trim();
        brand.Description = ApiValidation.TrimOptional(request.Description);
        brand.LogoUrl = ApiValidation.TrimOptional(request.LogoUrl);
        brand.IsActive = request.IsActive;
        brand.UpdatedAt = DateTimeOffset.UtcNow;
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
