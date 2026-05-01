using Aspire.Hosting;

namespace DarkKitchen.Testing.Aspire;

public sealed class AspireAppFixture : IAsyncLifetime
{
    private const string FixtureLockName = "DarkKitchen.AspireAppFixture";
    private static readonly string LockFilePath = Path.Combine(
        Path.GetTempPath(),
        $"{FixtureLockName}.lock");

    private static readonly object _localLock = new();

    private static readonly string[] TestAppHostArgs =
    [
        "DarkKitchen:IncludeWebApps=false",
        "DarkKitchen:UsePersistentVolumes=false"
    ];

    private DistributedApplication? _app;
    private FileStream? _lockFile;

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
        var timeout = Environment.GetEnvironmentVariable("CI") is null
            ? TimeSpan.FromMinutes(15)
            : TimeSpan.FromMinutes(30);

        var deadline = DateTime.UtcNow.Add(timeout);
        var lockAcquired = false;

        while (DateTime.UtcNow < deadline)
        {
            lock (_localLock)
            {
                try
                {
                    _lockFile = new FileStream(
                        LockFilePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        1,
                        useAsync: true);

                    lockAcquired = true;
                }
                catch (IOException)
                {
                    // Lock file is in use by another process
                }
            }

            if (lockAcquired)
                break;

            await Task.Delay(100);
        }

        if (!lockAcquired)
        {
            throw new TimeoutException($"Timed out waiting for the {FixtureLockName} test fixture lock.");
        }
    }

    private void ReleaseFixtureLock()
    {
        lock (_localLock)
        {
            if (_lockFile is not null)
            {
                try
                {
                    _lockFile.Dispose();
                }
                finally
                {
                    _lockFile = null;
                    try
                    {
                        File.Delete(LockFilePath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }
    }
}