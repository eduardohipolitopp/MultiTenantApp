using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Web.Resources;

namespace MultiTenantApp.Web.Models.DTOs
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
        
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public List<UserRuleDto> Rules { get; set; } = new List<UserRuleDto>();
    }
}
