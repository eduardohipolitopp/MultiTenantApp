using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Infrastructure.Persistence
{
    public class MongoDbInitializer
    {
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<MongoDbInitializer> _logger;

        public MongoDbInitializer(IMongoClient mongoClient, ILogger<MongoDbInitializer> logger)
        {
            _mongoClient = mongoClient;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var database = _mongoClient.GetDatabase("MultiTenantAuditDb");

                await CreateRequestResponseLogIndexes(database);
                await CreateAuditLogIndexes(database);

                _logger.LogInformation("MongoDB indexes created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing MongoDB.");
                throw;
            }
        }

        private async Task CreateRequestResponseLogIndexes(IMongoDatabase database)
        {
            var logsCollection = database.GetCollection<RequestResponseLog>("RequestResponseLogs");
            
            var indexKeysDefinition = Builders<RequestResponseLog>.IndexKeys
                .Ascending(x => x.TenantId)
                .Ascending(x => x.UserId)
                .Descending(x => x.Timestamp)
                .Ascending(x => x.Path)
                .Ascending(x => x.Method);

            var indexModel = new CreateIndexModel<RequestResponseLog>(indexKeysDefinition, new CreateIndexOptions { Name = "Tenant_User_Timestamp_Path_Method" });
            
            await logsCollection.Indexes.CreateOneAsync(indexModel);
        }

        private async Task CreateAuditLogIndexes(IMongoDatabase database)
        {
            var auditCollection = database.GetCollection<AuditLog>("AuditLogs");

            var indexKeysDefinition = Builders<AuditLog>.IndexKeys
                .Ascending(x => x.TenantId)
                .Ascending(x => x.EntityId)
                .Ascending(x => x.EntityType)
                .Descending(x => x.Timestamp);

            var indexModel = new CreateIndexModel<AuditLog>(indexKeysDefinition, new CreateIndexOptions { Name = "Tenant_Entity_Type_Timestamp" });

            await auditCollection.Indexes.CreateOneAsync(indexModel);
        }
    }
}
