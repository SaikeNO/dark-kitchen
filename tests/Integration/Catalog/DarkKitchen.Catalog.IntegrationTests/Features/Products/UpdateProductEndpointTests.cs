namespace DarkKitchen.Catalog.IntegrationTests.Features.Products;

[Collection(AspireAppCollection.Name)]
public sealed class UpdateProductEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanUpdateProduct()
    {
        using var catalog = await CreateManagerClientAsync();
        var scenario = await catalog.CreateProductScenarioAsync(NewSuffix());

        var updated = await catalog.UpdateProductAsync(
            scenario.Product.Id,
            new ProductRequest(
                scenario.Brand.Id,
                scenario.Category.Id,
                $"Updated Product {NewSuffix()}",
                "Updated description",
                "https://example.test/product.webp",
                42.90m,
                "PLN"));

        Assert.Equal(scenario.Product.Id, updated.Id);
        Assert.Equal(42.90m, updated.Price);
        Assert.Equal("Updated description", updated.Description);
    }

    [Fact]
    public async Task MissingProduct_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brand = await catalog.CreateBrandAsync($"Missing Product Brand {suffix}");
        var category = await catalog.CreateCategoryAsync(brand.Id, $"Missing Product Category {suffix}");

        using var response = await catalog.PutProductAsync(
            Guid.NewGuid(),
            new ProductRequest(brand.Id, category.Id, "Missing Product", null, null, 12, "PLN"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
