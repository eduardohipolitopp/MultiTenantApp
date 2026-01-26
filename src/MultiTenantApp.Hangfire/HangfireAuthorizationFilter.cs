using Hangfire.Dashboard;
using System.Security.Claims;

namespace MultiTenantApp.Hangfire
{
    /// <summary>
    /// Authorization filter for Hangfire dashboard.
    /// Requires authentication and admin role for access.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Check if user is authenticated
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            // Check if user has admin role or claim
            // You can customize this logic based on your requirements
            return httpContext.User.IsInRole("Admin") ||
                   httpContext.User.IsInRole("SystemAdmin") ||
                   httpContext.User.HasClaim(ClaimTypes.Role, "Admin") ||
                   httpContext.User.HasClaim("HangfireAccess", "true");
        }
    }
}
