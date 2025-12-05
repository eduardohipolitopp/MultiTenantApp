using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    public class Rule : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Navigation property
        public ICollection<UserRule> UserRules { get; set; } = new List<UserRule>();
    }
}
