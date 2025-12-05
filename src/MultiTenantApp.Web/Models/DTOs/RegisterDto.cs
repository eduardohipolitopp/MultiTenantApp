using System;
using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Web.Resources;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class RegisterDto
    {
        [Display(Name = "Email", ResourceType = typeof(SharedResource))]
        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "EmailAddress")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password", ResourceType = typeof(SharedResource))]
        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "TenantId", ResourceType = typeof(SharedResource))]
        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        public string TenantId { get; set; } = string.Empty;
    }
}
