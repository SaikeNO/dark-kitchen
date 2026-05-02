namespace DarkKitchen.Catalog.Features.Features.Uploads;

internal static class UploadedAssetAccess
{
    public static readonly IReadOnlyDictionary<string, string> ExtensionsByContentType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

    public static readonly IReadOnlyDictionary<string, string> ContentTypesByExtension =
        ExtensionsByContentType.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlySet<string> AllowedKinds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "brand-logo",
            "product-image"
        };

    public static bool IsSafeFileName(string fileName)
    {
        return fileName.Length > 0
            && fileName == Path.GetFileName(fileName)
            && fileName.All(character => char.IsAsciiLetterOrDigit(character) || character == '.');
    }
}
