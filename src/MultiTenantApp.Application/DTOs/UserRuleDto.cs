using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class UserRuleDto
    {
        public Guid Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        
        public Guid RuleId { get; set; }
        
        [Display(Name = "Name", ResourceType = typeof(SharedResource))]
        public string RuleName { get; set; } = string.Empty;
        
        [Display(Name = "Permission", ResourceType = typeof(SharedResource))]
        public string PermissionType { get; set; } = string.Empty;
    }
}
