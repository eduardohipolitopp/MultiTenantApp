using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
        {
            // TenantProvider logic is already in GetTenantId which checks claims and headers.
            // We just need to ensure it's available. 
            // Actually, TenantProvider implementation I wrote pulls from HttpContextAccessor.
            // So we might not strictly need a middleware to SET it, unless we want to enforce it or log it.
            
            // However, for the "Login" scenario where we might want to set it from body (too late for middleware usually) 
            // or header.
            
            // Let's just log it or do nothing if the provider is self-sufficient.
            // But wait, I implemented TenantProvider to check "X-Tenant-ID" header.
            // So it should work automatically.
            
            // One thing: If we want to support "Tenant from Body" for Login, we can't easily do it in middleware 
            // because reading body stream is tricky (needs buffering).
            // So we will rely on Header for unauthenticated requests, and Token for authenticated ones.
            
            await _next(context);
        }
    }
}
