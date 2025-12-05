using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class UpdateUserDto
    {
        [Display(Name = "Email", ResourceType = typeof(SharedResource))]
        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "EmailAddress")]
        public string Email { get; set; } = string.Empty;
    }
}
