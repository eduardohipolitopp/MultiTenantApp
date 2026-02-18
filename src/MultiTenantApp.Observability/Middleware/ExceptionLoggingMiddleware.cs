using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MultiTenantApp.Observability.Middleware;

/// <summary>
/// Middleware that captures all unhandled exceptions in the request pipeline,
/// logs them with full context (request path, method, trace id) and rethrows
/// so the application's exception handler can format the response.
/// Use as the innermost middleware (just before MapControllers/MapRazorPages) so it runs first when an exception occurs.
/// </summary>
public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            LogException(context, ex);
            throw;
        }
    }

    private void LogException(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        _logger.LogError(exception,
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}, User: {User}",
            traceId,
            path,
            method,
            context.User.Identity?.Name ?? "(anonymous)");

        // Structured properties for log aggregation (Serilog/OpenTelemetry)
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = traceId,
            ["RequestPath"] = path,
            ["RequestMethod"] = method,
            ["StatusCode"] = (int)HttpStatusCode.InternalServerError
        }))
        {
            _logger.LogError(exception, "Exception details: {ExceptionType}", exception.GetType().FullName);
        }
    }
}
