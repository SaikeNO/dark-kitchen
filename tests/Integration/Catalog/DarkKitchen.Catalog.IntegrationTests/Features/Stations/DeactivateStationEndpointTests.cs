namespace DarkKitchen.Catalog.IntegrationTests.Features.Stations;

[Collection(AspireAppCollection.Name)]
public sealed class DeactivateStationEndpointTests(AspireAppFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Manager_CanDeactivateStation()
    {
        using var catalog = await CreateManagerClientAsync();
        var suffix = NewSuffix();
        var station = await catalog.CreateStationAsync($"DS{suffix[..4]}", $"Deactivate Station {suffix}");

        var deactivated = await catalog.DeactivateStationAsync(station.Id);

        Assert.Equal(station.Id, deactivated.Id);
        Assert.False(deactivated.IsActive);
    }

    [Fact]
    public async Task MissingStation_ReturnsNotFound()
    {
        using var catalog = await CreateManagerClientAsync();

        using var response = await catalog.PostDeactivateStationAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
