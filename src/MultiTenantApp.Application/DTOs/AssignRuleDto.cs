using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class AssignRuleDto
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        public Guid RuleId { get; set; }
        
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        public int PermissionType { get; set; } // 1 = View, 2 = Edit
    }
}
