using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.MemoryStorage;
using Serilog;
using MultiTenantApp.Infrastructure.Jobs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Services;
using MultiTenantApp.Infrastructure.Persistence;
using MultiTenantApp.Hangfire;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

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

// Dependency Injection for Multi-tenancy and Infrastructure
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, MultiTenantApp.Infrastructure.Services.TenantProvider>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(MultiTenantApp.Infrastructure.Repositories.Repository<>));
builder.Services.AddScoped<IUnitOfWork, MultiTenantApp.Infrastructure.Repositories.UnitOfWork>();

// Dependency Injection for Jobs
builder.Services.AddScoped<SampleRecurringJob>();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Hangfire Dashboard (only in Development for security)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() },
        DashboardTitle = builder.Configuration["Hangfire:Dashboard:Title"] ?? "MultiTenantApp - Background Jobs"
    });
}

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
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during Hangfire startup.");
    }
}

app.Run();
