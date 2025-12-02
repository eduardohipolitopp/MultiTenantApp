using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class UpdateUserDto
    {
        [Display(Name = "Email", ResourceType = typeof(SharedResource))]
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
