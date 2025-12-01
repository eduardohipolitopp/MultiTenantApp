using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    public class Product : BaseTenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
