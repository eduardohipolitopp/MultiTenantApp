using System.ComponentModel.DataAnnotations;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class UpdateRuleDto
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;
    }
}
