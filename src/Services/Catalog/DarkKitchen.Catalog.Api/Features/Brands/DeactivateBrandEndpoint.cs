using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class DeactivateBrandEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid brandId,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var brand = await outbox.DbContext.Brands.FirstOrDefaultAsync(entity => entity.Id == brandId, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        brand.IsActive = false;
        brand.UpdatedAt = DateTimeOffset.UtcNow;
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(new Response(brand.Id, brand.Name, brand.Description, brand.LogoUrl, brand.IsActive));
    }

    public sealed record Response(
        Guid Id,
        string Name,
        string? Description,
        string? LogoUrl,
        bool IsActive);
}
