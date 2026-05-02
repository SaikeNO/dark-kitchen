namespace DarkKitchen.Storefront.Features.Features;

internal static class ApiValidation
{
    public static IResult Problem(params (string Key, string Error)[] errors)
    {
        return Results.ValidationProblem(errors.ToDictionary(
            error => error.Key,
            error => new[] { error.Error }));
    }

    public static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
