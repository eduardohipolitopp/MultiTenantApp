using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Infrastructure.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private Guid? _manualTenantId;

        public TenantProvider(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        public Guid? GetTenantId()
        {
            if (_manualTenantId != null) return _manualTenantId;

            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // First check claims
            var tenantClaim = context.User.Claims.FirstOrDefault(c => c.Type == "tenant_id");
            if (tenantClaim != null)
            {
                Guid.TryParse(tenantClaim.Value, out var guid);
                return guid;
            }
            // Then check headers (useful for initial requests or if not auth yet but tenant known)
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
            {
                Guid.TryParse(tenantId, out var guid);
                return guid;
            }

            return null;
        }

        public void SetTenantId(Guid tenantId)
        {
            _manualTenantId = tenantId;
        }

        public bool IsAdmin()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            // Check if user has Admin role claim
            return context.User.IsInRole("Admin");
        }

        public async Task<bool> IsAdminAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;

            // Use IServiceProvider to avoid circular dependency
            // We need to create a scope to get the DbContext
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Persistence.ApplicationDbContext>();
            
            // Check if user has Admin rule
            var hasAdminRule = await dbContext.UserRules
                .Include(ur => ur.Rule)
                .AnyAsync(ur => ur.UserId == userId && ur.Rule!.Name == "Admin");

            return hasAdminRule;
        }
    }
}
