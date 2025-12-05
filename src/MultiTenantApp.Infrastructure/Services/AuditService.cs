using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IMongoCollection<AuditLog> _auditLogs;

        public AuditService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase("MultiTenantAuditDb");
            _auditLogs = mongoDatabase.GetCollection<AuditLog>("AuditLogs");
            
            // Ensure indexes for performance
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var indexKeysDefinition = Builders<AuditLog>.IndexKeys
                .Ascending(x => x.TenantId)
                .Ascending(x => x.EntityId)
                .Ascending(x => x.EntityType)
                .Descending(x => x.Timestamp);

            var indexModel = new CreateIndexModel<AuditLog>(indexKeysDefinition);
            _auditLogs.Indexes.CreateOne(indexModel);
        }

        public async Task CreateAuditLogAsync(AuditLog auditLog)
        {
            await _auditLogs.InsertOneAsync(auditLog);
        }

        public async Task CreateAuditLogsBatchAsync(IEnumerable<AuditLog> auditLogs)
        {
            if (auditLogs != null && auditLogs.Any())
            {
                await _auditLogs.InsertManyAsync(auditLogs);
            }
        }
    }
}
