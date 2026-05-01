namespace DarkKitchen.Catalog.IntegrationTests.Features.Categories;

[Collection(AspireAppCollection.Name)]
public sealed class DeactivateCategoryEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanDeactivateCategory()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brand = await catalog.CreateBrandAsync($"Deactivate Category Brand {suffix}");
        var category = await catalog.CreateCategoryAsync(brand.Id, $"Deactivate Category {suffix}");

        var deactivated = await catalog.DeactivateCategoryAsync(category.Id);

        Assert.Equal(category.Id, deactivated.Id);
        Assert.False(deactivated.IsActive);
    }

    [Fact]
    public async Task MissingCategory_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostDeactivateCategoryAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
