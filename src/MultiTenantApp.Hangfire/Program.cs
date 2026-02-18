using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.MemoryStorage;
using OpenTelemetry;
using MultiTenantApp.Observability;
using MultiTenantApp.Infrastructure.Jobs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Services;
using MultiTenantApp.Infrastructure.Persistence;
using MultiTenantApp.Hangfire;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using MultiTenantApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serilog with OpenTelemetry
builder.Host.UseSerilogObservability();

// Database Context (needed for Hangfire PostgreSQL storage)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire Configuration
if (builder.Environment.IsProduction())
{
    builder.Services.AddHangfire(config => config
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
}
else
{
    builder.Services.AddHangfire(config => config.UseMemoryStorage());
}

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = builder.Configuration.GetValue<int>("Hangfire:Server:WorkerCount", Environment.ProcessorCount * 5);
});

// Configure MongoDB serialization conventions
MongoDbConfiguration.Configure();

// Register MongoDB Client as Singleton
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = MongoClientSettings.FromConnectionString(mongoConnectionString);
    
    // Add OpenTelemetry instrumentation
    settings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
    
    return new MongoClient(settings);
});

// Dependency Injection for Multi-tenancy and Infrastructure
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, MultiTenantApp.Infrastructure.Services.TenantProvider>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(MultiTenantApp.Infrastructure.Repositories.Repository<>));
builder.Services.AddScoped<IUnitOfWork, MultiTenantApp.Infrastructure.Repositories.UnitOfWork>();
builder.Services.AddScoped<IAuditService, MultiTenantApp.Infrastructure.Services.AuditService>();
builder.Services.AddScoped<ICurrentUserService, MultiTenantApp.Infrastructure.Services.CurrentUserService>();

// Dependency Injection for Jobs
builder.Services.AddScoped<SampleRecurringJob>();

// OpenTelemetry
builder.Services.AddOpenTelemetryObservability(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Test Observability
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Observability integrated successfully in MultiTenantApp.Hangfire at {Time}", DateTime.UtcNow);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Observability: log all HTTP and Hangfire job exceptions
app.UseExceptionLogging();
app.UseHangfireExceptionLogging();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = builder.Configuration["Hangfire:Dashboard:Title"] ?? "MultiTenantApp - Background Jobs"
});

app.UseAuthorization();

app.MapRazorPages();

// Configure recurring jobs
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var recurringJobManager = services.GetRequiredService<IRecurringJobManager>();
        var sampleJob = services.GetRequiredService<SampleRecurringJob>();

        // Example: Schedule a recurring job (runs every minute)
        // Uncomment to enable:
        // RecurringJob.AddOrUpdate("sample-job", () => sampleJob.ProcessSomething(), Cron.Minutely);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during Hangfire startup.");
    }
}

app.Run();
