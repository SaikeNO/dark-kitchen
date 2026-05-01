namespace DarkKitchen.Catalog.IntegrationTests.Features.Stations;

[Collection(AspireAppCollection.Name)]
public sealed class CreateStationEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanCreateStation()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();

        var station = await catalog.CreateStationAsync($"st{suffix[..4]}", $"Station {suffix}");

        Assert.NotEqual(Guid.Empty, station.Id);
        Assert.Equal($"ST{suffix[..4].ToUpperInvariant()}", station.Code);
        Assert.Equal($"Station {suffix}", station.Name);
    }

    [Fact]
    public async Task BlankDisplayColor_ReturnsValidationProblem()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostStationAsync(new StationRequest("GRILL", "Grill", " ", true));
        var problem = await response.ReadValidationProblemAsync();

        Assert.Contains("displayColor", problem.Errors.Keys);
    }

    [Fact]
    public async Task Operator_ReturnsForbidden()
    {
        using var catalog = await CreateOperatorClientAsync();

        using var response = await catalog.PostStationAsync(new StationRequest($"OP{NewSuffix()[..4]}", "Forbidden Station", "#2f7d57", true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
