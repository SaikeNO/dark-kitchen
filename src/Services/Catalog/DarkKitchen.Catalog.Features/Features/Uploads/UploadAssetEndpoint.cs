using Microsoft.Extensions.Options;

namespace DarkKitchen.Catalog.Features.Features.Uploads;

public static class UploadAssetEndpoint
{
    private const long MaxBytes = 2 * 1024 * 1024;

    public static async Task<IResult> HandleAsync(
        string kind,
        HttpContext httpContext,
        IOptions<CatalogUploadOptions> uploadOptions,
        CancellationToken ct)
    {
        if (!UploadedAssetAccess.AllowedKinds.Contains(kind))
        {
            return ApiValidation.Problem(("kind", "Unsupported upload kind."));
        }

        if (!httpContext.Request.HasFormContentType)
        {
            return ApiValidation.Problem(("file", "Multipart form file is required."));
        }

        var form = await httpContext.Request.ReadFormAsync(ct);
        var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0)
        {
            return ApiValidation.Problem(("file", "File is required."));
        }

        if (file.Length > MaxBytes)
        {
            return ApiValidation.Problem(("file", "File must be 2 MB or smaller."));
        }

        if (!UploadedAssetAccess.ExtensionsByContentType.TryGetValue(file.ContentType, out var extension))
        {
            return ApiValidation.Problem(("file", "Only jpg, png and webp images are supported."));
        }

        var targetDirectory = Path.Combine(uploadOptions.Value.RootPath, kind);
        Directory.CreateDirectory(targetDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var targetPath = Path.Combine(targetDirectory, fileName);
        await using (var stream = File.Create(targetPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var url = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/uploads/{kind}/{fileName}";
        return Results.Created(url, new Response(url));
    }

    public sealed record Response(string Url);
}
