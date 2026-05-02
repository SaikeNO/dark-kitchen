using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Categories;

public static class CreateCategoryEndpoint
{
    public static async Task<IResult> HandleAsync(
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var validation = await ValidateAsync(request, outbox.DbContext, ct);
        if (validation is not null)
        {
            return validation;
        }

        var category = Category.Create(
            request.BrandId,
            request.Name.Trim(),
            request.SortOrder,
            request.IsActive,
            DateTimeOffset.UtcNow);

        outbox.DbContext.Categories.Add(category);
        await outbox.PublishAsync(CatalogEventFactory.CategoryChanged(category, httpContext));
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Created($"/api/admin/categories/{category.Id}", Response.FromCategory(category));
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
