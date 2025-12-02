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
    }

    public abstract class BaseTenantEntity : BaseEntity, ITenantEntity
    {
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
