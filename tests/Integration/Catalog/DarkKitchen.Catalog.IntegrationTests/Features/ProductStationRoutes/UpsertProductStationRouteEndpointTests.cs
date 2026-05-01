namespace DarkKitchen.Catalog.IntegrationTests.Features.ProductStationRoutes;

[Collection(AspireAppCollection.Name)]
public sealed class UpsertProductStationRouteEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanUpsertProductStationRoute()
    {
        using var catalog = await CreateManagerClientAsync();
        var scenario = await catalog.CreateProductScenarioAsync(NewSuffix());

        var route = await catalog.UpsertStationRouteAsync(scenario.Product.Id, scenario.Station.Id);

        Assert.Equal(scenario.Product.Id, route.ProductId);
        Assert.Equal(scenario.Station.Id, route.StationId);
        Assert.Equal(scenario.Station.Code, route.StationCode);
    }

    [Fact]
    public async Task InactiveStation_ReturnsValidationProblem()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var brand = await catalog.CreateBrandAsync($"Route Brand {suffix}");
        var category = await catalog.CreateCategoryAsync(brand.Id, $"Route Category {suffix}");
        var product = await catalog.CreateProductAsync(brand.Id, category.Id, $"Route Product {suffix}");
        var station = await catalog.CreateStationAsync($"IR{suffix[..4]}", $"Inactive Route Station {suffix}", isActive: false);

        using var response = await catalog.PutStationRouteAsync(product.Id, new ProductStationRouteRequest(station.Id));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("stationId", problem.Errors.Keys);
    }

    [Fact]
    public async Task Operator_ReturnsForbidden()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.PutStationRouteAsync(Guid.NewGuid(), new ProductStationRouteRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
