using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var includeWebApps = !string.Equals(
    builder.Configuration["DarkKitchen:IncludeWebApps"],
    bool.FalseString,
    StringComparison.OrdinalIgnoreCase);

var usePersistentVolumes = !string.Equals(
    builder.Configuration["DarkKitchen:UsePersistentVolumes"],
    bool.FalseString,
    StringComparison.OrdinalIgnoreCase);

var useFixedWebPorts = string.Equals(
    builder.Configuration["DarkKitchen:UseFixedWebPorts"],
    bool.TrueString,
    StringComparison.OrdinalIgnoreCase);

var postgres = builder.AddPostgres("postgres");

if (usePersistentVolumes)
{
    postgres.WithDataVolume();
}

var catalogDb = postgres.AddDatabase("catalog-db", "darkkitchen_catalog");
var inventoryDb = postgres.AddDatabase("inventory-db", "darkkitchen_inventory");
var orderDb = postgres.AddDatabase("order-db", "darkkitchen_orders");
var storefrontDb = postgres.AddDatabase("storefront-db", "darkkitchen_storefront");
var kdsDb = postgres.AddDatabase("kds-db", "darkkitchen_kds");
var packingDb = postgres.AddDatabase("packing-db", "darkkitchen_packing");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var redis = builder.AddRedis("redis");

if (usePersistentVolumes)
{
    redis.WithDataVolume();
}

var catalogApi = builder.AddProject<Projects.DarkKitchen_Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(rabbitMq)
    .WaitFor(catalogDb)
    .WaitFor(rabbitMq);

var inventoryApi = builder.AddProject<Projects.DarkKitchen_Inventory_Api>("inventory-api")
    .WithReference(inventoryDb)
    .WithReference(rabbitMq)
    .WaitFor(inventoryDb)
    .WaitFor(rabbitMq);

var orderManagementApi = builder.AddProject<Projects.DarkKitchen_OrderManagement_Api>("order-management-api")
    .WithReference(orderDb)
    .WithReference(rabbitMq)
    .WaitFor(orderDb)
    .WaitFor(rabbitMq);

var storefrontApi = builder.AddProject<Projects.DarkKitchen_Storefront_Api>("storefront-api")
    .WithReference(storefrontDb)
    .WithReference(rabbitMq)
    .WithReference(redis)
    .WaitFor(storefrontDb)
    .WaitFor(rabbitMq)
    .WaitFor(redis);

var kdsApi = builder.AddProject<Projects.DarkKitchen_Kds_Api>("kds-api")
    .WithReference(kdsDb)
    .WithReference(rabbitMq)
    .WithReference(redis)
    .WaitFor(kdsDb)
    .WaitFor(rabbitMq)
    .WaitFor(redis);

var packingApi = builder.AddProject<Projects.DarkKitchen_Packing_Api>("packing-api")
    .WithReference(packingDb)
    .WithReference(rabbitMq)
    .WithReference(redis)
    .WaitFor(packingDb)
    .WaitFor(rabbitMq)
    .WaitFor(redis);

if (includeWebApps)
{
    var adminPanel = builder.AddViteApp("admin-panel", "../Web/admin-panel")
        .WithExternalHttpEndpoints()
        .WithReference(catalogApi)
        .WithEnvironment("VITE_API_BASE_URL", catalogApi.GetEndpoint("http"))
        .WaitFor(catalogApi);

    var inventoryPanel = builder.AddViteApp("inventory-panel", "../Web/inventory-panel")
        .WithExternalHttpEndpoints()
        .WithReference(inventoryApi)
        .WithEnvironment("VITE_API_BASE_URL", inventoryApi.GetEndpoint("http"))
        .WaitFor(inventoryApi);

    var storefront = builder.AddViteApp("storefront", "../Web/storefront")
        .WithExternalHttpEndpoints()
        .WithReference(storefrontApi)
        .WithEnvironment("VITE_API_BASE_URL", storefrontApi.GetEndpoint("http"))
        .WaitFor(storefrontApi);

    var kitchenApp = builder.AddViteApp("kitchen-app", "../Web/kitchen-app")
        .WithExternalHttpEndpoints()
        .WithReference(kdsApi)
        .WithEnvironment("VITE_API_BASE_URL", kdsApi.GetEndpoint("http"))
        .WaitFor(kdsApi);

    var packingTerminal = builder.AddViteApp("packing-terminal", "../Web/packing-terminal")
        .WithExternalHttpEndpoints()
        .WithReference(packingApi)
        .WithEnvironment("VITE_API_BASE_URL", packingApi.GetEndpoint("http"))
        .WaitFor(packingApi);

    if (useFixedWebPorts)
    {
        ConfigureFixedHttpPort(adminPanel, 5173);
        ConfigureFixedHttpPort(inventoryPanel, 5177);
        ConfigureFixedHttpPort(storefront, 5174);
        ConfigureFixedHttpPort(kitchenApp, 5175);
        ConfigureFixedHttpPort(packingTerminal, 5176);
    }
}

builder.Build().Run();

static IResourceBuilder<TResource> ConfigureFixedHttpPort<TResource>(
    IResourceBuilder<TResource> resource,
    int port)
    where TResource : IResourceWithEndpoints
{
    return resource.WithEndpoint("http", endpoint =>
    {
        endpoint.Port = port;
        endpoint.TargetPort = port;
        endpoint.IsExternal = true;
        endpoint.IsProxied = false;
    });
}
