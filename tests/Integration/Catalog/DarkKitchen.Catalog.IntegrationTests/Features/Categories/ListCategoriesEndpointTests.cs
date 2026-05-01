namespace DarkKitchen.Catalog.IntegrationTests.Features.Categories;

[Collection(AspireAppCollection.Name)]
public sealed class ListCategoriesEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Operator_CanListCategoriesScopedByBrand()
    {
        using var manager = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brandA = await manager.CreateBrandAsync($"Category Brand A {suffix}");
        var brandB = await manager.CreateBrandAsync($"Category Brand B {suffix}");
        var categoryA = await manager.CreateCategoryAsync(brandA.Id, $"Category A {suffix}");
        var categoryB = await manager.CreateCategoryAsync(brandB.Id, $"Category B {suffix}");

        using var catalog = await CreateOperatorClientAsync();
        var categories = await catalog.ListCategoriesAsync(brandA.Id);

        Assert.Contains(categories, category => category.Id == categoryA.Id);
        Assert.DoesNotContain(categories, category => category.Id == categoryB.Id);
    }

    [Fact]
    public async Task AnonymousUser_ReturnsUnauthorized()
    {
        using var client = await CreateAnonymousHttpClientAsync();

        using var response = await client.GetAsync("/api/admin/categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
