using Microsoft.AspNetCore.Hosting;

namespace DarkKitchen.Catalog.Features.Features.Uploads;

public static class UploadAssetEndpoint
{
    private const long MaxBytes = 2 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> ExtensionsByContentType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

    private static readonly IReadOnlySet<string> AllowedKinds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "brand-logo",
            "product-image"
        };

    public static async Task<IResult> HandleAsync(
        string kind,
        HttpContext httpContext,
        IWebHostEnvironment environment,
        CancellationToken ct)
    {
        if (!AllowedKinds.Contains(kind))
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

        if (!ExtensionsByContentType.TryGetValue(file.ContentType, out var extension))
        {
            return ApiValidation.Problem(("file", "Only jpg, png and webp images are supported."));
        }

        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        var relativeDirectory = Path.Combine("uploads", kind);
        var targetDirectory = Path.Combine(webRoot, relativeDirectory);
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
