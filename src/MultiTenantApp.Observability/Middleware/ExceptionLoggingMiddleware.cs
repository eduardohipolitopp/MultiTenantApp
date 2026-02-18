using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MultiTenantApp.Observability.Middleware;

/// <summary>
/// Middleware that captures all unhandled exceptions in the request pipeline,
/// logs them with full context (request path, method, trace id) and rethrows
/// so the application's exception handler can format the response.
/// When Serilog is configured, uses Log.ForContext to attach structured properties.
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
        var user = context.User.Identity?.Name ?? "(anonymous)";
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var exceptionType = exception.GetType().FullName ?? nameof(Exception);
        var message = string.IsNullOrWhiteSpace(exception.Message) ? exceptionType : exception.Message;

        if (IsSerilogEnabled())
        {
            Log.ForContext("TraceId", traceId)
                .ForContext("RequestPath", path)
                .ForContext("RequestMethod", method)
                .ForContext("User", user)
                .ForContext("StatusCode", statusCode)
                .ForContext("ExceptionType", exceptionType)
                .Error(exception, "{Message}", message);
        }
        else
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["TraceId"] = traceId,
                ["RequestPath"] = path,
                ["RequestMethod"] = method,
                ["User"] = user,
                ["StatusCode"] = statusCode,
                ["ExceptionType"] = exceptionType
            }))
            {
                _logger.LogError(exception, "{Message}", message);
            }
        }
    }

    private static bool IsSerilogEnabled()
    {
        if (Log.Logger == null)
            return false;
        return Log.Logger.GetType().Name != "SilentLogger";
    }
}
