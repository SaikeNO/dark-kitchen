namespace DarkKitchen.Catalog.IntegrationTests.Features.Brands;

[Collection(AspireAppCollection.Name)]
public sealed class UpdateBrandEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanUpdateBrand()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brand = await catalog.CreateBrandAsync($"Update Brand {suffix}");

        var updated = await catalog.UpdateBrandAsync(
            brand.Id,
            new BrandRequest($"Updated Brand {suffix}", "Updated", "https://example.test/logo.png", false));

        Assert.Equal(brand.Id, updated.Id);
        Assert.Equal($"Updated Brand {suffix}", updated.Name);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task MissingBrand_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PutBrandAsync(Guid.NewGuid(), new BrandRequest("Missing Brand", null, null, true));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
