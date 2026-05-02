using Microsoft.Extensions.Options;

namespace DarkKitchen.Catalog.Features.Features.Uploads;

public static class GetUploadedAssetEndpoint
{
    public static IResult Handle(
        string kind,
        string fileName,
        IOptions<CatalogUploadOptions> uploadOptions)
    {
        if (!UploadedAssetAccess.AllowedKinds.Contains(kind)
            || !UploadedAssetAccess.IsSafeFileName(fileName))
        {
            return Results.NotFound();
        }

        var extension = Path.GetExtension(fileName);
        if (!UploadedAssetAccess.ContentTypesByExtension.TryGetValue(extension, out var contentType))
        {
            return Results.NotFound();
        }

        var root = Path.GetFullPath(uploadOptions.Value.RootPath);
        var path = Path.GetFullPath(Path.Combine(root, kind, fileName));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
        {
            return Results.NotFound();
        }

        return Results.File(path, contentType, enableRangeProcessing: true);
    }
}
