namespace DarkKitchen.Catalog.IntegrationTests.Features.Categories;

[Collection(AspireAppCollection.Name)]
public sealed class UpdateCategoryEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanUpdateCategory()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brand = await catalog.CreateBrandAsync($"Update Category Brand {suffix}");
        var category = await catalog.CreateCategoryAsync(brand.Id, $"Old Category {suffix}");

        var updated = await catalog.UpdateCategoryAsync(
            category.Id,
            new CategoryRequest(brand.Id, $"Updated Category {suffix}", 20, false));

        Assert.Equal(category.Id, updated.Id);
        Assert.Equal($"Updated Category {suffix}", updated.Name);
        Assert.Equal(20, updated.SortOrder);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task MissingCategory_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();
        var brand = await catalog.CreateBrandAsync($"Missing Category Brand {NewSuffix()}");

        using var response = await catalog.PutCategoryAsync(Guid.NewGuid(), new CategoryRequest(brand.Id, "Missing Category", 10, true));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
