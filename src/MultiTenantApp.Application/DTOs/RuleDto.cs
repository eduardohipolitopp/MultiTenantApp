using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class RuleDto
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Role", ResourceType = typeof(SharedResource))]
        public string Name { get; set; } = string.Empty;
    }
}
