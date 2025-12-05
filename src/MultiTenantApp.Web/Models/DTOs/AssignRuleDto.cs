using System;
using System.ComponentModel.DataAnnotations;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class AssignRuleDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "RuleId is required")]
        public Guid RuleId { get; set; }

        [Required(ErrorMessage = "PermissionType is required")]
        public int PermissionType { get; set; } // 1 = View, 2 = Edit
    }
}
