using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MultiTenantApp.Observability;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Configures Serilog for observability, including enrichment and console output.
    /// Redirects logs to OpenTelemetry SDK via writeToProviders: true.
    /// </summary>
    public static IHostBuilder UseSerilogObservability(this IHostBuilder host, Action<ObservabilityOptions>? configureOptions = null)
    {
        host.UseSerilog((context, services, loggerConfiguration) =>
        {
            var options = GetOptions(context.Configuration, configureOptions);
            
            loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("service.name", options.ServiceName)
                .Enrich.WithProperty("service.version", options.ServiceVersion)
                .Enrich.WithProperty("service.environment", options.Environment);

            if (options.ResourceAttributes != null)
            {
                foreach (var attr in options.ResourceAttributes)
                {
                    loggerConfiguration.Enrich.WithProperty(attr.Key, attr.Value);
                }
            }

            if (options.Console.Enabled)
            {
                loggerConfiguration.WriteTo.Console(outputTemplate: options.Console.OutputTemplate ?? "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }
        }, writeToProviders: true);

        return host;
    }

    /// <summary>
    /// Configures OpenTelemetry metrics, tracing, and logging.
    /// </summary>
    public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services, IConfiguration configuration, Action<ObservabilityOptions>? configureOptions = null)
    {
        var options = GetOptions(configuration, configureOptions);
        options.Validate();

        var otelBuilder = services.AddOpenTelemetry()
            .ConfigureResource(resource => 
            {
                resource.AddService(options.ServiceName!, serviceVersion: options.ServiceVersion)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = options.Environment ?? "Unknown"
                    });

                if (options.ResourceAttributes != null)
                {
                    resource.AddAttributes(options.ResourceAttributes);
                }
            });

        if (options.EnableTracing)
        {
            otelBuilder.WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opt => opt.RecordException = true)
                    .AddHttpClientInstrumentation(opt => opt.RecordException = true)
                    .AddEntityFrameworkCoreInstrumentation(opt => 
                    {
                        opt.SetDbStatementForText = true;
                        opt.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSqlClientInstrumentation(opt => opt.RecordException = true)
                    .AddRedisInstrumentation()
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
                    .AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = new Uri(options.OtlpEndpoint!);
                        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            });
        }

        if (options.EnableMetrics)
        {
            otelBuilder.WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = new Uri(options.OtlpEndpoint!);
                        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            });
        }

        if (options.EnableLogging)
        {
            otelBuilder.WithLogging(logging =>
            {
                logging.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(options.OtlpEndpoint!);
                });
            });
        }

        return services;
    }

    private static ObservabilityOptions GetOptions(IConfiguration configuration, Action<ObservabilityOptions>? configureOptions)
    {
        var options = new ObservabilityOptions();
        
        // 1. Bind from configuration (appsettings)
        configuration.GetSection(ObservabilityOptions.SectionName).Bind(options);
        
        // 2. Apply code-based configuration (overrides appsettings)
        configureOptions?.Invoke(options);

        // 3. Auto-discover defaults
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        
        options.ServiceName ??= assembly.GetName().Name ?? "UnknownService";
        options.ServiceVersion ??= assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion 
                                    ?? assembly.GetName().Version?.ToString() 
                                    ?? "1.0.0";
        options.Environment ??= System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        return options;
    }
}
