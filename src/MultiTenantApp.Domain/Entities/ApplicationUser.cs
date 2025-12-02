using Microsoft.AspNetCore.Identity;
using MultiTenantApp.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantApp.Domain.Entities
{
    public class ApplicationUser : IdentityUser, ITenantEntity
    {
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
