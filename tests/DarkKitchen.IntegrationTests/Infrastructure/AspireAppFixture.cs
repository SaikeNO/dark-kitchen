using Aspire.Hosting;

namespace DarkKitchen.IntegrationTests.Infrastructure;

public sealed class AspireAppFixture : IAsyncLifetime
{
    private static readonly string[] TestAppHostArgs =
    [
        "DarkKitchen:IncludeWebApps=false",
        "DarkKitchen:UsePersistentVolumes=false"
    ];

    private DistributedApplication? _app;

    public static TimeSpan DefaultTimeout { get; } =
        Environment.GetEnvironmentVariable("CI") is null
            ? TimeSpan.FromSeconds(90)
            : TimeSpan.FromMinutes(5);

    public DistributedApplication App =>
        _app ?? throw new InvalidOperationException("The Aspire application has not been started.");

    public async Task InitializeAsync()
    {
        var cancellationToken = CancellationToken.None;
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.DarkKitchen_AppHost>(
            TestAppHostArgs,
            cancellationToken);

        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter("Aspire.Hosting.Dcp", LogLevel.Warning);
            logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        });

        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await builder.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await _app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
    }

    public async Task DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    public HttpClient CreateHttpClient(string resourceName) => App.CreateHttpClient(resourceName);

    public async Task WaitForHealthyAsync(string resourceName)
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        await App.ResourceNotifications.WaitForResourceHealthyAsync(resourceName, cts.Token);
    }
}
