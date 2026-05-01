using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Features.Ingredients;

public static class UpdateIngredientEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid ingredientId,
        Request request,
        IDbContextOutbox<CatalogDbContext> outbox,
        CancellationToken ct)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return validation;
        }

        var ingredient = await outbox.DbContext.Ingredients.FirstOrDefaultAsync(entity => entity.Id == ingredientId, ct);
        if (ingredient is null)
        {
            return Results.NotFound();
        }

        ingredient.Update(
            request.Name.Trim(),
            request.Unit.Trim(),
            request.IsActive,
            DateTimeOffset.UtcNow);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Ok(Response.FromIngredient(ingredient));
    }

    private static IResult? Validate(Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiValidation.Problem(("name", "Ingredient name is required."));
        }

        return string.IsNullOrWhiteSpace(request.Unit)
            ? ApiValidation.Problem(("unit", "Ingredient unit is required."))
            : null;
    }

    public sealed record Request(string Name, string Unit, bool IsActive);

    public sealed record Response(Guid Id, string Name, string Unit, bool IsActive)
    {
        public static Response FromIngredient(Ingredient ingredient)
        {
            return new Response(ingredient.Id, ingredient.Name, ingredient.Unit, ingredient.IsActive);
        }
    }
}
