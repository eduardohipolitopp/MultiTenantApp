using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class CreateRuleDto
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        [Display(Name = "Name", ResourceType = typeof(SharedResource))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description", ResourceType = typeof(SharedResource))]
        public string Description { get; set; } = string.Empty;
    }
}
