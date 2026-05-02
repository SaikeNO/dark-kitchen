namespace DarkKitchen.Catalog.Features.Features.Uploads;

public sealed class CatalogUploadOptions
{
    public string RootPath { get; set; } = DefaultRootPath();

    public static string DefaultRootPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var root = string.IsNullOrWhiteSpace(localAppData)
            ? Path.GetTempPath()
            : localAppData;

        return Path.Combine(root, "DarkKitchen", "Catalog", "uploads");
    }
}
