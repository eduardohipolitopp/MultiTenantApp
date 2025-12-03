using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenantApp.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IdempotentAttribute : Attribute, IAsyncActionFilter
    {
        private const string HeaderName = "Idempotency-Key";
        private const int CacheDurationMinutes = 60;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var idempotencyKey))
            {

                var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                var cacheKey = $"Idempotency_{idempotencyKey}";

                if (cache.TryGetValue(cacheKey, out IActionResult cachedResult))
                {
                    context.Result = cachedResult;
                    return;
                }

                var executedContext = await next();

                if (executedContext.Result is ObjectResult objectResult && (objectResult.StatusCode >= 200 && objectResult.StatusCode < 300))
                {
                    cache.Set(cacheKey, objectResult, TimeSpan.FromMinutes(CacheDurationMinutes));
                }
                else if (executedContext.Result is StatusCodeResult statusCodeResult && (statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300))
                {
                    cache.Set(cacheKey, statusCodeResult, TimeSpan.FromMinutes(CacheDurationMinutes));
                }
            }
            else
            {
                await next();
            }
        }
    }
}
