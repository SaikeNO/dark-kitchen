namespace DarkKitchen.Catalog.Features.Features;

internal static class ApiValidation
{
    public static IResult Problem(params (string Key, string Error)[] errors)
    {
        return Results.ValidationProblem(
            errors
                .GroupBy(error => error.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Error).ToArray(), StringComparer.Ordinal));
    }

    public static string NormalizeCurrency(string currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "PLN" : currency.Trim().ToUpperInvariant();
    }

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
