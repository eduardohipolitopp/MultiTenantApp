using Microsoft.AspNetCore.Http;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Infrastructure.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _manualTenantId;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetTenantId()
        {
            if (_manualTenantId != null) return _manualTenantId;

            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // First check claims
            var tenantClaim = context.User.Claims.FirstOrDefault(c => c.Type == "tenant_id");
            if (tenantClaim != null) return tenantClaim.Value;

            // Then check headers (useful for initial requests or if not auth yet but tenant known)
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
            {
                return tenantId.ToString();
            }

            return null;
        }

        public void SetTenantId(string tenantId)
        {
            _manualTenantId = tenantId;
        }
    }
}
