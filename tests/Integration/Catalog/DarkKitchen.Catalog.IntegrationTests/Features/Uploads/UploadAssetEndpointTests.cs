namespace DarkKitchen.Catalog.IntegrationTests.Features.Uploads;

[Collection(AspireAppCollection.Name)]
public sealed class UploadAssetEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanUploadBrandLogo()
    {
        using var catalog = await CreateManagerClientAsync();
        using var content = new MultipartFormDataContent();
        using var file = new ByteArrayContent(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII="));
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(file, "file", "logo.png");

        using var response = await catalog.HttpClient.PostAsync("/api/admin/uploads/brand-logo", content);
        await response.AssertSuccessAsync();
        var uploaded = await response.ReadJsonAsync<UploadResponse>();

        Assert.Contains("/uploads/brand-logo/", uploaded.Url, StringComparison.Ordinal);
    }

    public sealed record UploadResponse(string Url);
}
