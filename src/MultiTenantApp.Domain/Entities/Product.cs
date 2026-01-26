using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;

namespace MultiTenantApp.Domain.Entities
{
    /// <summary>
    /// Represents a product entity in the system.
    /// Supports logical delete - when deleted, IsDeleted flag is set instead of removing from database.
    /// </summary>
    [LogicalDelete]
    public class Product : BaseTenantEntity
    {
        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product price
        /// </summary>
        public decimal Price { get; set; }
    }
}
