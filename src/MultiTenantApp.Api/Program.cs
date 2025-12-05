using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MultiTenantApp.Api.Middleware;
using MultiTenantApp.Application.Configuration;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Application.Services.Authentication;
using MultiTenantApp.Application.Services.Profile;
using MultiTenantApp.Application.Services.Tenants;
using MultiTenantApp.Application.Services.Users;
using MultiTenantApp.Infrastructure.Services;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Persistence;
using MultiTenantApp.Infrastructure.Repositories;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();

    var lokiUrl = context.Configuration["Loki:Url"];
    if (!string.IsNullOrEmpty(lokiUrl))
    {
        configuration.WriteTo.GrafanaLoki(lokiUrl);
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MultiTenantApp API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});

// Configuration Options
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection(CacheOptions.SectionName));
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection(RateLimitOptions.SectionName));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

// Redis Configuration
var cacheOptions = builder.Configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>();
if (cacheOptions?.Enabled == true)
{
    var redisConfig = ConfigurationOptions.Parse(cacheOptions.Redis.ConnectionString);
    redisConfig.ConnectTimeout = cacheOptions.Redis.ConnectTimeout;
    redisConfig.SyncTimeout = cacheOptions.Redis.SyncTimeout;
    redisConfig.AbortOnConnectFail = cacheOptions.Redis.AbortOnConnectFail;
    redisConfig.ConnectRetry = cacheOptions.Redis.ConnectRetry;

    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = cacheOptions.Redis.ConnectionString;
        options.InstanceName = "MultiTenantApp:";
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
    builder.Services.AddSingleton<IRateLimitService, RateLimitService>();
    builder.Services.AddScoped<CacheDecorator>();
}
else
{
    // Use in-memory distributed cache when Redis is disabled
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddScoped<CacheDecorator>();
}

// Dependency Injection
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<ITenantValidationService, TenantValidationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRuleService, RuleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddHttpContextAccessor();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MultiTenantApp.Api"))
            .AddAspNetCoreInstrumentation()
            //.AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(metricsProviderBuilder =>
    {
        metricsProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MultiTenantApp.Api"))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment()) // Always show swagger for demo
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var supportedCultures = new[] { "en-US", "pt-BR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");

// Culture middleware must be after authentication to access user claims
app.UseMiddleware<CultureMiddleware>();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();
app.UseMiddleware<TenantMiddleware>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
