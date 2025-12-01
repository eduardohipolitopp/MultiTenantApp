using Microsoft.AspNetCore.Identity;
using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    public class ApplicationUser : IdentityUser, ITenantEntity
    {
        public string TenantId { get; set; } = string.Empty;
    }
}
