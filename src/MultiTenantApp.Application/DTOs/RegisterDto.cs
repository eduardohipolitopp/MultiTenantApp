using System;
using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class RegisterDto
    {
        [Display(Name = "Email", ResourceType = typeof(SharedResource))]
        [Required]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password", ResourceType = typeof(SharedResource))]
        [Required]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "TenantId", ResourceType = typeof(SharedResource))]
        [Required]
        public string TenantId { get; set; } = string.Empty;
    }
}
