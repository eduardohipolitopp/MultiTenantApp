using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Infrastructure.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly IMongoCollection<AuditLog> _auditLogs;

        public AuditRepository(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase("MultiTenantAuditDb");
            _auditLogs = mongoDatabase.GetCollection<AuditLog>("AuditLogs");
        }

        public async Task<List<AuditLog>> GetEntityHistoryAsync(Guid entityId, string entityType, Guid tenantId)
        {
            var filter = Builders<AuditLog>.Filter.And(
                Builders<AuditLog>.Filter.Eq(x => x.TenantId, tenantId),
                Builders<AuditLog>.Filter.Eq(x => x.EntityId, entityId),
                Builders<AuditLog>.Filter.Eq(x => x.EntityType, entityType)
            );

            return await _auditLogs.Find(filter)
                .SortByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<(List<AuditLog> Items, long TotalCount)> GetAuditLogsAsync(
            Guid tenantId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Guid? userId = null,
            string? entityType = null,
            int page = 1,
            int pageSize = 20)
        {
            var builder = Builders<AuditLog>.Filter;
            var filter = builder.Eq(x => x.TenantId, tenantId);

            if (startDate.HasValue)
            {
                filter &= builder.Gte(x => x.Timestamp, startDate.Value);
            }

            if (endDate.HasValue)
            {
                filter &= builder.Lte(x => x.Timestamp, endDate.Value);
            }

            if (userId.HasValue)
            {
                filter &= builder.Eq(x => x.UserId, userId.Value);
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                filter &= builder.Eq(x => x.EntityType, entityType);
            }

            var totalCount = await _auditLogs.CountDocumentsAsync(filter);

            var items = await _auditLogs.Find(filter)
                .SortByDescending(x => x.Timestamp)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
