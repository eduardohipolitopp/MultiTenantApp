using Microsoft.AspNetCore.Mvc.Filters;
using MultiTenantApp.Application.Services;

namespace MultiTenantApp.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class InvalidateCacheAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _pattern;

        public InvalidateCacheAttribute(string pattern)
        {
            _pattern = pattern;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            if (executedContext.Exception == null && executedContext.Result is not Microsoft.AspNetCore.Mvc.BadRequestResult)
            {
                var cacheDecorator = context.HttpContext.RequestServices.GetService<CacheDecorator>();
                if (cacheDecorator != null)
                {
                    try
                    {
                        await cacheDecorator.InvalidateCacheByPatternAsync(_pattern);
                    }
                    catch
                    {
                        // Silently fail if cache is not available
                    }
                }
            }
        }
    }
}
