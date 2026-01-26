using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Services;

namespace MultiTenantApp.Api.Middleware
{
    /// <summary>
    /// Middleware to log HTTP requests and responses to MongoDB.
    /// Only logs endpoints that have the LogRequestResponseAttribute.
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        // Headers that should not be logged for security reasons
        private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Cookie",
            "X-API-Key",
            "X-Auth-Token"
        };

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var attribute = endpoint?.Metadata?.GetMetadata<LogRequestResponseAttribute>();

            // Only log if the attribute is present
            if (attribute == null)
            {
                await _next(context);
                return;
            }

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<RequestResponseLogService>();
            var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();

            var stopwatch = Stopwatch.StartNew();
            var log = new RequestResponseLog
            {
                Method = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                QueryString = context.Request.QueryString.Value,
                IpAddress = GetClientIpAddress(context),
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                Timestamp = DateTime.UtcNow
            };

            // Get tenant and user info
            var tenantId = tenantProvider.GetTenantId();
            if (tenantId.HasValue)
            {
                log.TenantId = tenantId.Value;
            }

            var userId = currentUserService.UserId;
            if (Guid.TryParse(userId, out var userIdGuid))
            {
                log.UserId = userIdGuid;
                log.UserName = currentUserService.UserName;
            }

            // Log request headers (sanitized)
            log.RequestHeaders = GetSanitizedHeaders(context.Request.Headers);

            // Log request body if enabled
            if (attribute.LogRequestBody && context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                var requestBody = await ReadRequestBodyAsync(context.Request);
                log.RequestBody = TruncateIfNeeded(requestBody, attribute.MaxBodyLength);
            }

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                log.ExceptionMessage = ex.Message;
                _logger.LogError(ex, "Error processing request {Path}", log.Path);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.DurationMs = stopwatch.ElapsedMilliseconds;
                log.StatusCode = context.Response.StatusCode;

                // Log response headers (sanitized)
                log.ResponseHeaders = GetSanitizedHeaders(context.Response.Headers);

                // Log response body if enabled
                if (attribute.LogResponseBody)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
                    log.ResponseBody = TruncateIfNeeded(responseBodyText, attribute.MaxBodyLength);
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
                else
                {
                    await responseBody.CopyToAsync(originalBodyStream);
                }

                context.Response.Body = originalBodyStream;

                // Save log asynchronously (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await logService.CreateLogAsync(log);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save request/response log");
                    }
                });
            }
        }

        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private static Dictionary<string, string> GetSanitizedHeaders(IHeaderDictionary headers)
        {
            var sanitized = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                if (!SensitiveHeaders.Contains(header.Key))
                {
                    sanitized[header.Key] = string.Join(", ", header.Value.ToArray());
                }
                else
                {
                    sanitized[header.Key] = "***REDACTED***";
                }
            }
            return sanitized;
        }

        private static string TruncateIfNeeded(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text ?? string.Empty;

            return text.Substring(0, maxLength) + "... [TRUNCATED]";
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',')[0]?.Trim() ?? string.Empty;
                }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
