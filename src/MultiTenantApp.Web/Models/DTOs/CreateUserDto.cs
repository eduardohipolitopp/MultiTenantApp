using System.ComponentModel.DataAnnotations;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class CreateUserDto
    {
        [Display(Name = "UserName")]
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "TenantId")]
        [Required(ErrorMessage = "TenantId is required")]
        public string TenantId { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }
}
