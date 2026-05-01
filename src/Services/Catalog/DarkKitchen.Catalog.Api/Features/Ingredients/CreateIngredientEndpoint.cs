using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Api.Features;

public static class CreateIngredientEndpoint
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

        var now = DateTimeOffset.UtcNow;
        var ingredient = new Ingredient
        {
            Name = request.Name.Trim(),
            Unit = request.Unit.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        outbox.DbContext.Ingredients.Add(ingredient);
        await outbox.SaveChangesAndFlushMessagesAsync(ct);

        return Results.Created($"/api/admin/ingredients/{ingredient.Id}", Response.FromIngredient(ingredient));
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
