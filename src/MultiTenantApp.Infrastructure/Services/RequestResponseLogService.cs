using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Infrastructure.Services
{
    /// <summary>
    /// Service for logging HTTP requests and responses to MongoDB.
    /// </summary>
    public class RequestResponseLogService
    {
        private readonly IMongoCollection<RequestResponseLog> _logs;

        public RequestResponseLogService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            var mongoClient = MongoDbConfiguration.CreateClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase("MultiTenantAuditDb");
            _logs = mongoDatabase.GetCollection<RequestResponseLog>("RequestResponseLogs");

            // Ensure indexes for performance
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var indexKeysDefinition = Builders<RequestResponseLog>.IndexKeys
                .Ascending(x => x.TenantId)
                .Ascending(x => x.UserId)
                .Descending(x => x.Timestamp)
                .Ascending(x => x.Path)
                .Ascending(x => x.Method);

            var indexModel = new CreateIndexModel<RequestResponseLog>(indexKeysDefinition);
            _logs.Indexes.CreateOne(indexModel);
        }

        /// <summary>
        /// Creates a request/response log entry.
        /// </summary>
        public async Task CreateLogAsync(RequestResponseLog log)
        {
            await _logs.InsertOneAsync(log);
        }
    }
}
