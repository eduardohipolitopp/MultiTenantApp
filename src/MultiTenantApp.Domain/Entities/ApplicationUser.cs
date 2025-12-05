using Microsoft.AspNetCore.Identity;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantApp.Domain.Entities
{
    public class ApplicationUser : IdentityUser, ITenantEntity
    {
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        
        // Profile fields
        public string? FullName { get; set; }
        public int? Age { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public SupportedLanguage PreferredLanguage { get; set; } = SupportedLanguage.EnglishUS;
        
        // Navigation property for user rules
        public ICollection<UserRule> UserRules { get; set; } = new List<UserRule>();
    }
}
