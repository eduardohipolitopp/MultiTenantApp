using MultiTenantApp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantApp.Domain.Common
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }

    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Indicates if the entity has been logically deleted.
        /// Only used when the entity has the LogicalDeleteAttribute.
        /// </summary>
        public bool IsDeleted { get; set; } = false;
        
        /// <summary>
        /// Timestamp when the entity was logically deleted.
        /// Only used when the entity has the LogicalDeleteAttribute.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }

    public abstract class BaseTenantEntity : BaseEntity, ITenantEntity
    {
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
