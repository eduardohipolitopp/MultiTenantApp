using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Configure HttpClient to point to API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://api:8080") }); // Docker internal URL usually, or localhost if running outside. 
// Note: For server-side Blazor running in Docker, "api" is the service name. 
// If running locally without docker, it might be localhost:5000.
// We will use configuration or environment variables ideally.
// For this task, let's assume Docker environment "http://api:8080" but fallback to localhost for local dev if needed.
// Actually, better to use "http://localhost:5000" for client side? No, this is Server Side Blazor.
// It runs on the server. So it needs to reach the API.
// If both are in docker compose, "http://api:8080" is correct.
// Let's stick to a config value.

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
