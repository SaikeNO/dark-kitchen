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
            new BrandRequest(
                $"Updated Brand {suffix}",
                "Updated",
                "https://example.test/logo.png",
                ["updated.example.test"],
                "Updated hero",
                "Updated subtitle",
                "#111111",
                "#222222",
                "#ffffff",
                "#000000",
                false));

        Assert.Equal(brand.Id, updated.Id);
        Assert.Equal($"Updated Brand {suffix}", updated.Name);
        Assert.Equal("#111111", updated.PrimaryColor);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task MissingBrand_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PutBrandAsync(
            Guid.NewGuid(),
            new BrandRequest("Missing Brand", null, null, [], null, null, null, null, null, null, true));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
