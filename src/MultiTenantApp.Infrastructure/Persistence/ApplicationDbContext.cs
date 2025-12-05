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
        public DbSet<Rule> Rules { get; set; }
        public DbSet<UserRule> UserRules { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Multi-tenancy query filters
            // Note: EF Core query filters do not support async methods, so admin bypass must be done explicitly
            // using IgnoreQueryFilters() when needed for admin operations
            builder.Entity<Product>().HasQueryFilter(e =>
                _tenantProvider.GetTenantId() == null || e.TenantId == _tenantProvider.GetTenantId());

            builder.Entity<ApplicationUser>().HasQueryFilter(e =>
                _tenantProvider.GetTenantId() == null || e.TenantId == _tenantProvider.GetTenantId());


            builder.Entity<UserRule>().HasQueryFilter(e =>
                _tenantProvider.GetTenantId() == null || e.TenantId == _tenantProvider.GetTenantId());

            // Configure Rule entity
            builder.Entity<Rule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => new { e.Name }).IsUnique();
            });

            // Configure UserRule entity
            builder.Entity<UserRule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRules)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Rule)
                    .WithMany(r => r.UserRules)
                    .HasForeignKey(e => e.RuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.RuleId }).IsUnique();
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != null && tenantId != new Guid())
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
