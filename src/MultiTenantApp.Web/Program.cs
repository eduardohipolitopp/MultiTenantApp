using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using MultiTenantApp.Web.Interfaces;
using MultiTenantApp.Web.Services;
using OpenTelemetry;
using MultiTenantApp.Observability;
using Microsoft.AspNetCore.DataProtection;
using MultiTenantApp.Web.Configuration;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

// Serilog with OpenTelemetry
builder.Host.UseSerilogObservability();

// Redis Configuration
var cacheOptions = builder.Configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>();
if (cacheOptions?.Enabled == true)
{
    var redisConfig = ConfigurationOptions.Parse(cacheOptions.Redis.ConnectionString);
    redisConfig.ConnectTimeout = cacheOptions.Redis.ConnectTimeout;
    redisConfig.SyncTimeout = cacheOptions.Redis.SyncTimeout;
    redisConfig.AbortOnConnectFail = cacheOptions.Redis.AbortOnConnectFail;
    redisConfig.ConnectRetry = cacheOptions.Redis.ConnectRetry;

    var redis = ConnectionMultiplexer.Connect(redisConfig);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = cacheOptions.Redis.ConnectionString;
        options.InstanceName = "MultiTenantApp:";
    });
    builder.Services.AddScoped<CacheDecorator>();
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
}
else
{
    // Use in-memory distributed cache when Redis is disabled
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddScoped<CacheDecorator>();
}


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; })
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    });
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
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ThemeService>();

// OpenTelemetry
builder.Services.AddOpenTelemetryObservability(builder.Configuration);

var app = builder.Build();

// Test Observability
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Observability integrated successfully in MultiTenantApp.Web at {Time}", DateTime.UtcNow);

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

// Observability: log all exceptions before they are handled
app.UseExceptionLogging();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
