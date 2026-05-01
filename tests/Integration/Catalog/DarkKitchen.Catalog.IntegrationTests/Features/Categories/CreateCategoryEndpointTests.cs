namespace DarkKitchen.Catalog.IntegrationTests.Features.Categories;

[Collection(AspireAppCollection.Name)]
public sealed class CreateCategoryEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanCreateCategoryForBrand()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brand = await catalog.CreateBrandAsync($"Category Brand {suffix}");

        var category = await catalog.CreateCategoryAsync(brand.Id, $"Category {suffix}");

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(brand.Id, category.BrandId);
        Assert.Equal($"Category {suffix}", category.Name);
    }

    [Fact]
    public async Task UnknownBrand_ReturnsValidationProblem()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostCategoryAsync(new CategoryRequest(Guid.NewGuid(), "Missing Brand Category", 1, true));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("brandId", problem.Errors.Keys);
    }

    [Fact]
    public async Task Operator_ReturnsForbidden()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.PostCategoryAsync(new CategoryRequest(Guid.NewGuid(), "Forbidden Category", 1, true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
