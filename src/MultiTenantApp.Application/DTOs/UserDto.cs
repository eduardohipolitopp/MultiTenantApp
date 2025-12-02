using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Email", ResourceType = typeof(SharedResource))]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Name", ResourceType = typeof(SharedResource))]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "TenantId", ResourceType = typeof(SharedResource))]
        public string TenantId { get; set; } = string.Empty;
    }
}
