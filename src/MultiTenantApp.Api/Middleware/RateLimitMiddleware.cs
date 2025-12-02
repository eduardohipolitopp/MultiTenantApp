using System.Net;
using Microsoft.Extensions.Options;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.Configuration;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Api.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly RateLimitOptions _options;

        public RateLimitMiddleware(
            RequestDelegate next,
            ILogger<RateLimitMiddleware> logger,
            IOptions<RateLimitOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService, ITenantProvider tenantProvider)
        {
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            // Check if endpoint has DisableRateLimit attribute
            var endpointMetadata = context.GetEndpoint();
            if (endpointMetadata?.Metadata?.GetMetadata<DisableRateLimitAttribute>() != null)
            {
                await _next(context);
                return;
            }

            var endpoint = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            // Check endpoint-specific rate limit first
            var endpointPolicy = _options.Endpoints.FirstOrDefault(e => 
                endpoint.StartsWith(e.Endpoint, StringComparison.OrdinalIgnoreCase) &&
                (e.HttpMethods.Length == 0 || e.HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase)));

            if (endpointPolicy != null)
            {
                var endpointKey = $"endpoint:{endpoint}:{method}";
                var endpointLimit = endpointPolicy.Limit;
                var endpointWindow = TimeSpan.FromMinutes(endpointPolicy.WindowMinutes);

                var endpointResult = await rateLimitService.CheckRateLimitAsync(endpointKey, endpointLimit, endpointWindow);
                if (!endpointResult.IsAllowed)
                {
                    await HandleRateLimitExceeded(context, endpointResult, "Endpoint");
                    return;
                }
            }

            // Check per-tenant rate limit
            if (_options.PerTenant.Enabled)
            {
                var tenantId = tenantProvider.GetTenantId();
                if (tenantId.HasValue)
                {
                    var tenantKey = $"tenant:{tenantId}";
                    var tenantLimit = _options.PerTenant.Limit;
                    var tenantWindow = TimeSpan.FromMinutes(_options.PerTenant.WindowMinutes);

                    var tenantResult = await rateLimitService.CheckRateLimitAsync(tenantKey, tenantLimit, tenantWindow);
                    if (!tenantResult.IsAllowed)
                    {
                        await HandleRateLimitExceeded(context, tenantResult, "Tenant");
                        return;
                    }
                }
            }

            // Check per-user rate limit
            if (_options.PerUser.Enabled && context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.Identity.Name ?? context.User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userKey = $"user:{userId}";
                    var userLimit = _options.PerUser.Limit;
                    var userWindow = TimeSpan.FromMinutes(_options.PerUser.WindowMinutes);

                    var userResult = await rateLimitService.CheckRateLimitAsync(userKey, userLimit, userWindow);
                    if (!userResult.IsAllowed)
                    {
                        await HandleRateLimitExceeded(context, userResult, "User");
                        return;
                    }
                }
            }

            // Check per-IP rate limit
            if (_options.PerIp.Enabled)
            {
                var ipAddress = GetClientIpAddress(context);
                var ipKey = $"ip:{ipAddress}";
                var ipLimit = _options.PerIp.Limit;
                var ipWindow = TimeSpan.FromMinutes(_options.PerIp.WindowMinutes);

                var ipResult = await rateLimitService.CheckRateLimitAsync(ipKey, ipLimit, ipWindow);
                if (!ipResult.IsAllowed)
                {
                    await HandleRateLimitExceeded(context, ipResult, "IP");
                    return;
                }
            }

            // Check global rate limit
            if (_options.Global.Enabled)
            {
                var globalKey = "global";
                var globalLimit = _options.Global.Limit;
                var globalWindow = TimeSpan.FromMinutes(_options.Global.WindowMinutes);

                var globalResult = await rateLimitService.CheckRateLimitAsync(globalKey, globalLimit, globalWindow);
                if (!globalResult.IsAllowed)
                {
                    await HandleRateLimitExceeded(context, globalResult, "Global");
                    return;
                }
            }

            await _next(context);
        }

        private async Task HandleRateLimitExceeded(HttpContext context, RateLimitResult result, string limitType)
        {
            _logger.LogWarning(
                "Rate limit exceeded for {LimitType}. Path: {Path}, RequestCount: {RequestCount}",
                limitType, context.Request.Path, result.RequestCount);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            // Add rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = result.RequestCount.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
            
            if (result.RetryAfter.HasValue)
            {
                context.Response.Headers["Retry-After"] = ((int)result.RetryAfter.Value.TotalSeconds).ToString();
            }

            var response = new
            {
                error = "Rate limit exceeded",
                limitType = limitType,
                message = $"Too many requests. Please retry after {result.RetryAfter?.TotalSeconds ?? 0} seconds.",
                remaining = result.Remaining,
                retryAfter = result.RetryAfter?.TotalSeconds
            };

            await context.Response.WriteAsJsonAsync(response);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for X-Forwarded-For header (proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Check for X-Real-IP header
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Use remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
