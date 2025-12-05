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
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            ITenantProvider tenantProvider,
            IAuditService auditService,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _tenantProvider = tenantProvider;
            _auditService = auditService;
            _currentUserService = currentUserService;
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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != null && tenantId != Guid.Empty)
            {
                foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.TenantId = tenantId.Value;
                    }
                }
            }

            var auditEntries = OnBeforeSaveChanges(tenantId);
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(auditEntries);
            
            return result;
        }

        private List<AuditLog> OnBeforeSaveChanges(Guid? tenantId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditLog>();
            var userId = _currentUserService.UserId;
            var userName = _currentUserService.UserName ?? "System"; // Fallback if username not available

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                // Check for SkipAudit attribute
                var type = entry.Entity.GetType();
                if (Attribute.IsDefined(type, typeof(MultiTenantApp.Domain.Attributes.SkipAuditAttribute)))
                    continue;

                var auditEntry = new AuditLog
                {
                    EntityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue is Guid id ? id : Guid.Empty,
                    EntityType = type.Name,
                    UserId = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
                    UserName = userName,
                    TenantId = tenantId ?? Guid.Empty,
                    Timestamp = DateTime.UtcNow,
                    Changes = new Dictionary<string, FieldChange>()
                };

                // If EntityId is empty (e.g. Added state), it will be updated in OnAfterSaveChanges if possible, 
                // but for Added entities the ID is usually generated by DB or constructor. 
                // In this project BaseEntity generates ID in constructor, so it should be available.
                if (entry.Entity is BaseEntity baseEntity)
                {
                    auditEntry.EntityId = baseEntity.Id;
                }

                if (entry.State == EntityState.Added)
                {
                    auditEntry.Action = "Create";
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsTemporary || property.Metadata.IsPrimaryKey()) continue;
                        
                        auditEntry.Changes.Add(property.Metadata.Name, new FieldChange { NewValue = property.CurrentValue });
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditEntry.Action = "Delete";
                    foreach (var property in entry.Properties)
                    {
                        if (property.Metadata.IsPrimaryKey()) continue;
                        auditEntry.Changes.Add(property.Metadata.Name, new FieldChange { OldValue = property.OriginalValue });
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditEntry.Action = "Update";
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified)
                        {
                            auditEntry.Changes.Add(property.Metadata.Name, new FieldChange 
                            { 
                                OldValue = property.OriginalValue, 
                                NewValue = property.CurrentValue 
                            });
                        }
                    }
                }

                if (auditEntry.Changes.Count > 0 || auditEntry.Action == "Delete")
                {
                    auditEntries.Add(auditEntry);
                }
            }

            return auditEntries;
        }

        private async Task OnAfterSaveChanges(List<AuditLog> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            await _auditService.CreateAuditLogsBatchAsync(auditEntries);
        }
    }
}
