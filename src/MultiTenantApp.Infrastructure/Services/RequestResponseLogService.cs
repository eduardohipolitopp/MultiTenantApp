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

        public RequestResponseLogService(IMongoClient mongoClient)
        {
            var mongoDatabase = mongoClient.GetDatabase("MultiTenantAuditDb");
            _logs = mongoDatabase.GetCollection<RequestResponseLog>("RequestResponseLogs");
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
