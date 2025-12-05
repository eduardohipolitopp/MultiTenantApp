using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using MultiTenantApp.Web.Interfaces;
using MultiTenantApp.Web.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.OpenTelemetry;


var builder = WebApplication.CreateBuilder(args);

// Serilog with OpenTelemetry
var otlpEndpointHttp = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]?.Replace("4317", "4318") ?? "http://localhost:4318";
var appName = "MultiTenantApp.Web";
var appVersion = "1.0.0";
var environment = builder.Environment.EnvironmentName;

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .Enrich.WithMachineName()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("service.name", appName)
        .Enrich.WithProperty("service.version", appVersion)
        .Enrich.WithProperty("service.environment", environment)
        .WriteTo.Console()
        .WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = $"{otlpEndpointHttp}/v1/logs";
            options.Protocol = OtlpProtocol.HttpProtobuf;
            options.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"] = appName,
                ["service.version"] = appVersion,
                ["deployment.environment"] = environment
            };
        });
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// TokenProvider
builder.Services.AddScoped<ITokenProvider, TokenProvider>();

// HttpClient base configurado
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// AuthenticatedHttpClient que usa o HttpClient configurado
builder.Services.AddScoped<AuthenticatedHttpClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ApiClient");
    var tokenProvider = sp.GetRequiredService<ITokenProvider>();
    return new AuthenticatedHttpClient(httpClient, tokenProvider);
});

// Seus serviços
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRuleService, RuleService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ThemeService>();

// OpenTelemetry
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("MultiTenantApp.Web")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }))
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
            });
    })
    .WithMetrics(metricsProviderBuilder =>
    {
        metricsProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
            });
    })
    .WithLogging(logging =>
    {
        logging.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        });
    });

var app = builder.Build();

var supportedCultures = new[] { "en-US", "pt-BR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(localizationOptions);

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
