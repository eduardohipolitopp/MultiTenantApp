using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Bson;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Services;
using static MongoDB.Driver.WriteConcern;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

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

                        var newVal = NormalizeForMongo(property.CurrentValue);
                        auditEntry.Changes.Add(property.Metadata.Name, new FieldChange { NewValue = newVal });
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditEntry.Action = "Delete";
                    foreach (var property in entry.Properties)
                    {
                        if (property.Metadata.IsPrimaryKey()) continue;

                        var oldVal = NormalizeForMongo(property.OriginalValue);
                        auditEntry.Changes.Add(property.Metadata.Name, new FieldChange { OldValue = oldVal });
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditEntry.Action = "Update";
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified)
                        {
                            if (property.OriginalValue == property.CurrentValue)
                            {
                                continue;
                            }

                            var oldVal = NormalizeForMongo(property.OriginalValue);
                            var newVal = NormalizeForMongo(property.CurrentValue);
                            auditEntry.Changes.Add(property.Metadata.Name, new FieldChange
                            {
                                OldValue = oldVal,
                                NewValue = newVal
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
        private object? NormalizeForMongo(object? value)
        {
            if (value == null) return null;

            if (value is Guid g)
                return new BsonBinaryData(g, GuidRepresentation.Standard);

            if (value is string s && Guid.TryParse(s, out var parsedFromString))
                return new BsonBinaryData(parsedFromString, GuidRepresentation.Standard);

            if (value is JObject jObj)
            {
                var dict = jObj.Properties()
                               .ToDictionary(p => p.Name, p => (object?)NormalizeJToken(p.Value));
                return dict;
            }

            if (value is JsonElement je)
            {
                return NormalizeJsonElement(je);
            }

            if (value is IDictionary dictObj)
            {
                var result = new Dictionary<string, object?>();
                foreach (var key in dictObj.Keys)
                {
                    var k = key?.ToString() ?? string.Empty;
                    var v = dictObj[key];
                    result[k] = NormalizeForMongo(v);
                }
                return result;
            }

            if (value is IEnumerable enumerable && !(value is string))
            {
                var list = new List<object?>();
                foreach (var item in enumerable)
                {
                    list.Add(NormalizeForMongo(item));
                }
                return list;
            }

            return value;
        }

        private object? NormalizeJToken(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) return null;
            if (token.Type == JTokenType.Guid)
            {
                var g = token.ToObject<Guid>();
                return new BsonBinaryData(g, GuidRepresentation.Standard);
            }
            if (token.Type == JTokenType.String && Guid.TryParse(token.ToString(), out var parsed))
            {
                return new BsonBinaryData(parsed, GuidRepresentation.Standard);
            }
            if (token is JObject o)
            {
                return o.Properties().ToDictionary(p => p.Name, p => (object?)NormalizeJToken(p.Value));
            }
            if (token is JArray arr)
            {
                return arr.Select(NormalizeJToken).ToList();
            }
            return ((JValue)token).Value;
        }

        private object? NormalizeJsonElement(JsonElement je)
        {
            switch (je.ValueKind)
            {
                case JsonValueKind.String:
                    var s = je.GetString();
                    if (!string.IsNullOrEmpty(s) && Guid.TryParse(s, out var parsed)) return new BsonBinaryData(parsed, GuidRepresentation.Standard);
                    return s;
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object?>();
                    foreach (var prop in je.EnumerateObject())
                    {
                        dict[prop.Name] = NormalizeJsonElement(prop.Value);
                    }
                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach (var item in je.EnumerateArray())
                        list.Add(NormalizeJsonElement(item));
                    return list;
                case JsonValueKind.Number:
                    if (je.TryGetInt64(out var l)) return l;
                    if (je.TryGetDouble(out var d)) return d;
                    return je.GetRawText();
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.Null: return null;
                default: return je.GetRawText();
            }
        }


        private async Task OnAfterSaveChanges(List<AuditLog> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            await _auditService.CreateAuditLogsBatchAsync(auditEntries);
        }
    }
}
