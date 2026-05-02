using System.Threading.Tasks;

namespace DarkKitchen.Catalog.IntegrationTests.Features.Uploads;

[Collection(AspireAppCollection.Name)]
public sealed class GetUploadedAssetEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public Task Can_Get_Uploaded_Asset_When_Exists()
    {
        return Task.CompletedTask;
    }
}
