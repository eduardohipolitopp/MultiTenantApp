using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Enums;
using System.Security.Claims;

namespace MultiTenantApp.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _ruleName;
        private readonly PermissionType _permissionType;

        public RequirePermissionAttribute(string ruleName, PermissionType permissionType)
        {
            _ruleName = ruleName;
            _permissionType = permissionType;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices
                .GetService(typeof(IPermissionService)) as IPermissionService;

            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var hasPermission = await permissionService.HasPermissionAsync(userId, _ruleName, _permissionType);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
