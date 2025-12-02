using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantProvider _tenantProvider;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Query filters - only apply if tenant is set
            builder.Entity<Product>().HasQueryFilter(e => 
                _tenantProvider.GetTenantId() == null || e.TenantId == _tenantProvider.GetTenantId());
            
            builder.Entity<ApplicationUser>().HasQueryFilter(e => 
                _tenantProvider.GetTenantId() == null || e.TenantId == _tenantProvider.GetTenantId());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != null && tenantId !=  new Guid())
            {
                foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.TenantId = tenantId.Value;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
