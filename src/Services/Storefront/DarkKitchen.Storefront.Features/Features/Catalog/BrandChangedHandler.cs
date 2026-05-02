using DarkKitchen.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Features.Catalog;

public static class BrandChangedHandler
{
    public static Task Handle(IntegrationEventEnvelope<BrandChanged> message, StorefrontDbContext db, CancellationToken ct)
    {
        return UpsertAsync(message.Payload, db, ct);
    }

    public static async Task UpsertAsync(BrandChanged payload, StorefrontDbContext db, CancellationToken ct)
    {
        var snapshot = await db.BrandSites.FirstOrDefaultAsync(brand => brand.BrandId == payload.BrandId, ct);
        var now = DateTimeOffset.UtcNow;
        if (snapshot is null)
        {
            db.BrandSites.Add(BrandSiteSnapshot.Create(
                payload.BrandId,
                payload.Name,
                payload.Description,
                payload.LogoUrl,
                payload.Domains,
                payload.HeroTitle,
                payload.HeroSubtitle,
                payload.PrimaryColor,
                payload.AccentColor,
                payload.BackgroundColor,
                payload.TextColor,
                payload.IsActive,
                now));
        }
        else
        {
            snapshot.Update(
                payload.Name,
                payload.Description,
                payload.LogoUrl,
                payload.Domains,
                payload.HeroTitle,
                payload.HeroSubtitle,
                payload.PrimaryColor,
                payload.AccentColor,
                payload.BackgroundColor,
                payload.TextColor,
                payload.IsActive,
                now);
        }

        await db.SaveChangesAsync(ct);
    }
}
