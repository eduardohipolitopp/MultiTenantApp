using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Web.Resources;
using MultiTenantApp.Web.Models.Enums;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public int? Age { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public SupportedLanguage PreferredLanguage { get; set; } = SupportedLanguage.EnglishUS;
        public bool EmailConfirmed { get; set; }
    }

    public class UpdateProfileDto
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        [Range(1, 150)]
        public int? Age { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        public SupportedLanguage PreferredLanguage { get; set; } = SupportedLanguage.EnglishUS;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "EmailAddress")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "EmailAddress")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required")]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
