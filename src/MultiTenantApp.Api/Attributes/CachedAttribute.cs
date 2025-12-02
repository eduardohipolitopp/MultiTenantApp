using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MultiTenantApp.Domain.Interfaces;
using System.Text;

namespace MultiTenantApp.Api.Attributes
{
    /// <summary>
    /// Attribute to automatically cache controller action responses
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _durationMinutes;
        private readonly bool _varyByTenant;
        private readonly bool _varyByUser;
        private readonly string[] _varyByQueryParams;

        public CachedAttribute(
            int durationMinutes = 5, 
            bool varyByTenant = true, 
            bool varyByUser = false,
            params string[] varyByQueryParams)
        {
            _durationMinutes = durationMinutes;
            _varyByTenant = varyByTenant;
            _varyByUser = varyByUser;
            _varyByQueryParams = varyByQueryParams ?? Array.Empty<string>();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetService<ICacheService>();
            
            if (cacheService == null)
            {
                await next();
                return;
            }

            var cacheKey = GenerateCacheKey(context);
            var cachedResponse = await cacheService.GetAsync<object>(cacheKey);

            if (cachedResponse != null)
            {
                context.Result = new OkObjectResult(cachedResponse);
                return;
            }

            var executedContext = await next();

            if (executedContext.Result is OkObjectResult okResult && okResult.Value != null)
            {
                var expiration = TimeSpan.FromMinutes(_durationMinutes);
                await cacheService.SetAsync(cacheKey, okResult.Value, expiration);
            }
        }

        private string GenerateCacheKey(ActionExecutingContext context)
        {
            var keyBuilder = new StringBuilder();
            
            // Base key: controller + action
            keyBuilder.Append($"action:{context.RouteData.Values["controller"]}:{context.RouteData.Values["action"]}");

            // Vary by tenant
            if (_varyByTenant)
            {
                var tenantProvider = context.HttpContext.RequestServices.GetService<ITenantProvider>();
                var tenantId = tenantProvider?.GetTenantId();
                if (tenantId.HasValue)
                {
                    keyBuilder.Append($":tenant:{tenantId}");
                }
            }

            // Vary by user
            if (_varyByUser && context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.HttpContext.User.Identity.Name;
                keyBuilder.Append($":user:{userId}");
            }

            // Vary by query parameters
            foreach (var param in _varyByQueryParams)
            {
                if (context.HttpContext.Request.Query.TryGetValue(param, out var value))
                {
                    keyBuilder.Append($":{param}:{value}");
                }
            }

            // Vary by route parameters
            foreach (var routeValue in context.RouteData.Values.Where(v => v.Key != "controller" && v.Key != "action"))
            {
                keyBuilder.Append($":{routeValue.Key}:{routeValue.Value}");
            }

            return keyBuilder.ToString();
        }
    }
}
