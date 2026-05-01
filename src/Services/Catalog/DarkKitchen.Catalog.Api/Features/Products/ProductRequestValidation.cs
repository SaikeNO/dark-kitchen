using Microsoft.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

internal static class ProductRequestValidation
{
    public static async Task<IResult?> ValidateAsync(
        Guid brandId,
        Guid categoryId,
        string name,
        decimal price,
        string currency,
        CatalogDbContext db,
        CancellationToken ct)
    {
        if (brandId == Guid.Empty)
        {
            return ApiValidation.Problem(("brandId", "Brand is required."));
        }

        if (categoryId == Guid.Empty)
        {
            return ApiValidation.Problem(("categoryId", "Category is required."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return ApiValidation.Problem(("name", "Product name is required."));
        }

        if (!string.Equals(ApiValidation.NormalizeCurrency(currency), "PLN", StringComparison.Ordinal))
        {
            return ApiValidation.Problem(("currency", "Only PLN is supported in the MVP."));
        }

        if (price < 0)
        {
            return ApiValidation.Problem(("price", "Price cannot be negative."));
        }

        var categoryMatchesBrand = await db.Categories
            .AnyAsync(category => category.Id == categoryId && category.BrandId == brandId, ct);

        return categoryMatchesBrand ? null : ApiValidation.Problem(("categoryId", "Category must belong to the selected brand."));
    }
}
