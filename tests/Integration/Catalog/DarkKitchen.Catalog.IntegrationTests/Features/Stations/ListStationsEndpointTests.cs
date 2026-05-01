namespace DarkKitchen.Catalog.IntegrationTests.Features.Stations;

[Collection(AspireAppCollection.Name)]
public sealed class ListStationsEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Operator_CanListStations()
    {
        using var manager = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var created = await manager.CreateStationAsync($"LS{suffix[..4]}", $"List Station {suffix}");

        using var catalog = await CreateOperatorClientAsync();
        var stations = await catalog.ListStationsAsync();

        Assert.Contains(stations, station => station.Id == created.Id);
    }

    [Fact]
    public async Task AnonymousUser_ReturnsUnauthorized()
    {
        using var client = await CreateAnonymousHttpClientAsync();

        using var response = await client.GetAsync("/api/admin/stations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
