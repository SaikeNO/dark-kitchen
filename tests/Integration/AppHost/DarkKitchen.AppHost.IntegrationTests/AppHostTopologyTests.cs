namespace DarkKitchen.AppHost.IntegrationTests;

public sealed class AppHostTopologyTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AppHost_DefaultTopology_IncludesApiInfrastructureAndWebApps()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.DarkKitchen_AppHost>();
        var resourceNames = builder.Resources.Select(resource => resource.Name).ToHashSet(StringComparer.Ordinal);

        Assert.Contains("postgres", resourceNames);
        Assert.Contains("rabbitmq", resourceNames);
        Assert.Contains("redis", resourceNames);
        Assert.Contains("catalog-api", resourceNames);
        Assert.Contains("inventory-api", resourceNames);
        Assert.Contains("order-management-api", resourceNames);
        Assert.Contains("storefront-api", resourceNames);
        Assert.Contains("kds-api", resourceNames);
        Assert.Contains("packing-api", resourceNames);
        Assert.Contains("admin-panel", resourceNames);
        Assert.Contains("inventory-panel", resourceNames);
        Assert.Contains("storefront", resourceNames);
        Assert.Contains("kitchen-app", resourceNames);
        Assert.Contains("packing-terminal", resourceNames);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AppHost_TestTopology_CanSkipWebApps()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.DarkKitchen_AppHost>(
            ["DarkKitchen:IncludeWebApps=false"]);
        var resourceNames = builder.Resources.Select(resource => resource.Name).ToHashSet(StringComparer.Ordinal);

        Assert.Contains("catalog-api", resourceNames);
        Assert.DoesNotContain("admin-panel", resourceNames);
        Assert.DoesNotContain("inventory-panel", resourceNames);
        Assert.DoesNotContain("storefront", resourceNames);
        Assert.DoesNotContain("kitchen-app", resourceNames);
        Assert.DoesNotContain("packing-terminal", resourceNames);
    }
}
