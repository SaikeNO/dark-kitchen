using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace DarkKitchen.ServiceDefaults;

public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string CorrelationHeaderName = "X-Correlation-Id";
    private const string CorrelationTagName = "correlation.id";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.Services.TryAddSingleton<CorrelationIdAccessor>();
        builder.Services.TryAddSingleton<CorrelationIdHandler>();
        builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
            http.AddHttpMessageHandler<CorrelationIdHandler>();
        });

        builder.AddDefaultHealthChecks();

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var correlationId = GetOrCreateCorrelationId(context);
            var accessor = context.RequestServices.GetRequiredService<CorrelationIdAccessor>();
            accessor.CorrelationId = correlationId;

            Activity.Current?.SetTag(CorrelationTagName, correlationId);
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationHeaderName] = correlationId;
                return Task.CompletedTask;
            });

            try
            {
                using (app.Logger.BeginScope(new Dictionary<string, object>
                {
                    [CorrelationTagName] = correlationId
                }))
                {
                    await next(context);
                }
            }
            finally
            {
                accessor.CorrelationId = null;
            }
        });

        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapHealthChecks(HealthEndpointPath);

        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live")
        });

        return app;
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var header = context.Request.Headers[CorrelationHeaderName].FirstOrDefault();
        return Guid.TryParse(header, out var correlationId) && correlationId != Guid.Empty
            ? correlationId.ToString("D")
            : Guid.NewGuid().ToString("D");
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

}

internal sealed class CorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> CurrentCorrelationId = new();

    public string? CorrelationId
    {
        get => CurrentCorrelationId.Value;
        set => CurrentCorrelationId.Value = value;
    }
}

internal sealed class CorrelationIdHandler(CorrelationIdAccessor accessor) : DelegatingHandler
{
    private const string CorrelationHeaderName = "X-Correlation-Id";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (accessor.CorrelationId is { Length: > 0 } correlationId
            && !request.Headers.Contains(CorrelationHeaderName))
        {
            request.Headers.Add(CorrelationHeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
