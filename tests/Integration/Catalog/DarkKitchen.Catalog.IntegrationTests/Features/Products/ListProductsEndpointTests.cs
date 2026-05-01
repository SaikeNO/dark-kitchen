namespace DarkKitchen.Catalog.IntegrationTests.Features.Products;

[Collection(AspireAppCollection.Name)]
public sealed class ListProductsEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Operator_CanListProductsScopedByBrand()
    {
        using var manager = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brandA = await manager.CreateBrandAsync($"List Product Brand A {suffix}");
        var brandB = await manager.CreateBrandAsync($"List Product Brand B {suffix}");
        var categoryA = await manager.CreateCategoryAsync(brandA.Id, $"List Product Category A {suffix}");
        var categoryB = await manager.CreateCategoryAsync(brandB.Id, $"List Product Category B {suffix}");
        var productA = await manager.CreateProductAsync(brandA.Id, categoryA.Id, $"List Product A {suffix}");
        var productB = await manager.CreateProductAsync(brandB.Id, categoryB.Id, $"List Product B {suffix}");

        using var catalog = await CreateOperatorClientAsync();
        var products = await catalog.ListProductsAsync(brandA.Id);

        Assert.Contains(products, product => product.Id == productA.Id);
        Assert.DoesNotContain(products, product => product.Id == productB.Id);
    }

    [Fact]
    public async Task AnonymousUser_ReturnsUnauthorized()
    {
        using var client = await CreateAnonymousHttpClientAsync();

        using var response = await client.GetAsync("/api/admin/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
