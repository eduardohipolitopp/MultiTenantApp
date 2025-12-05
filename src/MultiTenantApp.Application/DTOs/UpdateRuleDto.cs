using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class UpdateRuleDto
    {
        [Display(Name = "Name", ResourceType = typeof(SharedResource))]
        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        public string Name { get; set; } = string.Empty;
    }
}
