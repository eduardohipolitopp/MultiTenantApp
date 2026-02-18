using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IMongoCollection<AuditLog> _auditLogs;

        public AuditService(IMongoClient mongoClient)
        {
            var mongoDatabase = mongoClient.GetDatabase("MultiTenantAuditDb");
            _auditLogs = mongoDatabase.GetCollection<AuditLog>("AuditLogs");
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
