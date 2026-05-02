namespace DarkKitchen.Catalog.IntegrationTests.Features.Brands;

[Collection(AspireAppCollection.Name)]
public sealed class CreateBrandEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanCreateBrand()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();

        var brand = await catalog.CreateBrandAsync($"Create Brand {suffix}");

        Assert.NotEqual(Guid.Empty, brand.Id);
        Assert.Equal($"Create Brand {suffix}", brand.Name);
        Assert.True(brand.IsActive);
    }

    [Fact]
    public async Task Operator_ReturnsForbidden()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.PostBrandAsync(NewBrandRequest($"Forbidden Brand {NewSuffix()}"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BlankName_ReturnsValidationProblem()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostBrandAsync(NewBrandRequest(" "));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("name", problem.Errors.Keys);
    }

    private static BrandRequest NewBrandRequest(string name)
    {
        return new BrandRequest(name, null, null, [], null, null, null, null, null, null, true);
    }
}
