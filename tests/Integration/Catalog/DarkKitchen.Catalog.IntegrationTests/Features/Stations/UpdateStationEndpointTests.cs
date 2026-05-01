namespace DarkKitchen.Catalog.IntegrationTests.Features.Stations;

[Collection(AspireAppCollection.Name)]
public sealed class UpdateStationEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanUpdateStation()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var station = await catalog.CreateStationAsync($"US{suffix[..4]}", $"Old Station {suffix}");

        var updated = await catalog.UpdateStationAsync(
            station.Id,
            new StationRequest($"nu{suffix[..4]}", $"Updated Station {suffix}", "#123456", false));

        Assert.Equal(station.Id, updated.Id);
        Assert.Equal($"NU{suffix[..4].ToUpperInvariant()}", updated.Code);
        Assert.Equal($"Updated Station {suffix}", updated.Name);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task MissingStation_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PutStationAsync(Guid.NewGuid(), new StationRequest("MISSING", "Missing Station", "#123456", true));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
