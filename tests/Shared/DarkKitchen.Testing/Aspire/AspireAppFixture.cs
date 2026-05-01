using Aspire.Hosting;

namespace DarkKitchen.Testing.Aspire;

public sealed class AspireAppFixture : IAsyncLifetime
{
    private const string FixtureLockName = "DarkKitchen.AspireAppFixture";

    private static readonly string[] TestAppHostArgs =
    [
        "DarkKitchen:IncludeWebApps=false",
        "DarkKitchen:UsePersistentVolumes=false"
    ];

    private DistributedApplication? _app;
    private Semaphore? _fixtureLock;
    private bool _hasFixtureLock;

    public static TimeSpan DefaultTimeout { get; } =
        Environment.GetEnvironmentVariable("CI") is null
            ? TimeSpan.FromSeconds(90)
            : TimeSpan.FromMinutes(5);

    public DistributedApplication App =>
        _app ?? throw new InvalidOperationException("The Aspire application has not been started.");

    public async Task InitializeAsync()
    {
        await AcquireFixtureLockAsync();

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

        ReleaseFixtureLock();
    }

    public HttpClient CreateHttpClient(string resourceName) => App.CreateHttpClient(resourceName);

    public async Task WaitForHealthyAsync(string resourceName)
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        await App.ResourceNotifications.WaitForResourceHealthyAsync(resourceName, cts.Token);
    }

    private async Task AcquireFixtureLockAsync()
    {
        _fixtureLock = new Semaphore(1, 1, FixtureLockName);
        var timeout = Environment.GetEnvironmentVariable("CI") is null
            ? TimeSpan.FromMinutes(15)
            : TimeSpan.FromMinutes(30);

        _hasFixtureLock = await Task.Run(() => _fixtureLock.WaitOne(timeout));
        if (!_hasFixtureLock)
        {
            _fixtureLock.Dispose();
            _fixtureLock = null;
            throw new TimeoutException($"Timed out waiting for the {FixtureLockName} test fixture lock.");
        }
    }

    private void ReleaseFixtureLock()
    {
        if (_fixtureLock is null)
        {
            return;
        }

        try
        {
            if (_hasFixtureLock)
            {
                _fixtureLock.Release();
            }
        }
        finally
        {
            _fixtureLock.Dispose();
            _fixtureLock = null;
            _hasFixtureLock = false;
        }
    }
}
