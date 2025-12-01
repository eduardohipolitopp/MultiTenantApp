using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty; // e.g. "tenant-a"
    }
}
