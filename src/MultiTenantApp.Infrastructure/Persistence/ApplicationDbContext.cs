using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

            // Apply global query filter for ITenantEntity
            builder.Entity<Product>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
            // Users are also tenant specific usually, but Identity handles it differently sometimes.
            // For this requirement: "Todas entidades possuem TenantId"
            builder.Entity<ApplicationUser>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());

            // Tenant entity itself might not need filtering if we want to list them for login, 
            // but usually we filter it too if we are strict. 
            // However, for login dropdown we might need a separate way to fetch tenants or just not filter Tenant entity.
            // Let's assume Tenant entity is NOT filtered by TenantId (it defines the tenant).
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!string.IsNullOrEmpty(tenantId))
            {
                foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.TenantId = tenantId;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
