using System;
using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Web.Resources;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class LoginDto
    {
        [Display(Name = "Email", ResourceType = typeof(SharedResource))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password", ResourceType = typeof(SharedResource))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "TenantId", ResourceType = typeof(SharedResource))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        public string TenantId { get; set; } = string.Empty;
    }
}
