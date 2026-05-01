using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class UpdateCategoryEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid categoryId,
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var db = outbox.DbContext;
        var validation = await ValidateAsync(request, db, ct);
        if (validation is not null)
        {
            return validation;
        }

        var category = await db.Categories.FirstOrDefaultAsync(entity => entity.Id == categoryId, ct);
        if (category is null)
        {
            return Results.NotFound();
        }

        category.BrandId = request.BrandId;
        category.Name = request.Name.Trim();
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTimeOffset.UtcNow;
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(Response.FromCategory(category));
    }

    private static async Task<IResult?> ValidateAsync(Request request, CatalogDbContext db, CancellationToken ct)
    {
        if (request.BrandId == Guid.Empty)
        {
            return ApiValidation.Problem(("brandId", "Brand is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiValidation.Problem(("name", "Category name is required."));
        }

        var brandExists = await db.Brands.AnyAsync(brand => brand.Id == request.BrandId, ct);
        return brandExists ? null : ApiValidation.Problem(("brandId", "Brand does not exist."));
    }

    public sealed record Request(Guid BrandId, string Name, int SortOrder, bool IsActive);

    public sealed record Response(Guid Id, Guid BrandId, string Name, int SortOrder, bool IsActive)
    {
        public static Response FromCategory(Category category)
        {
            return new Response(category.Id, category.BrandId, category.Name, category.SortOrder, category.IsActive);
        }
    }
}
