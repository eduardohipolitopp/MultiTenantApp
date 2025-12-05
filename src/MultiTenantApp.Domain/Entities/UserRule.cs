using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantApp.Domain.Entities
{
    public class UserRule : BaseTenantEntity
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey(nameof(Rule))]
        public Guid RuleId { get; set; }
        
        public PermissionType PermissionType { get; set; }
        
        // Navigation properties
        public ApplicationUser? User { get; set; }
        public Rule? Rule { get; set; }
    }
}
