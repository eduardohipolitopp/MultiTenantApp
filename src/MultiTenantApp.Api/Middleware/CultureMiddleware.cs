using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Extensions;
using System.Globalization;
using System.Security.Claims;

namespace MultiTenantApp.Api.Middleware
{
    public class CultureMiddleware
    {
        private readonly RequestDelegate _next;

        public CultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            // Get user from claims
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var cultureInfo = user.PreferredLanguage.ToCultureInfo();

                        // Set culture for the current thread
                        CultureInfo.CurrentCulture = cultureInfo;
                        CultureInfo.CurrentUICulture = cultureInfo;
                    }
                }
                catch
                {
                    // If user not found or error, use default culture
                }
            }

            await _next(context);
        }
    }
}
