namespace DarkKitchen.Catalog.IntegrationTests.Features.Products;

[Collection(AspireAppCollection.Name)]
public sealed class CreateProductEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanCreateProduct()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brand = await catalog.CreateBrandAsync($"Product Brand {suffix}");
        var category = await catalog.CreateCategoryAsync(brand.Id, $"Product Category {suffix}");

        var product = await catalog.CreateProductAsync(brand.Id, category.Id, $"Product {suffix}");

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(brand.Id, product.BrandId);
        Assert.Equal(category.Id, product.CategoryId);
        Assert.Equal("PLN", product.Currency);
        Assert.False(product.IsActive);
    }

    [Fact]
    public async Task CategoryFromDifferentBrand_ReturnsValidationProblem()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brandA = await catalog.CreateBrandAsync($"Product Brand A {suffix}");
        var brandB = await catalog.CreateBrandAsync($"Product Brand B {suffix}");
        var categoryB = await catalog.CreateCategoryAsync(brandB.Id, $"Product Category B {suffix}");

        using var response = await catalog.PostProductAsync(new ProductRequest(brandA.Id, categoryB.Id, "Invalid Product", null, null, 10, "PLN"));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("categoryId", problem.Errors.Keys);
    }

    [Fact]
    public async Task Operator_ReturnsForbidden()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.PostProductAsync(new ProductRequest(Guid.NewGuid(), Guid.NewGuid(), "Forbidden Product", null, null, 10, "PLN"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
