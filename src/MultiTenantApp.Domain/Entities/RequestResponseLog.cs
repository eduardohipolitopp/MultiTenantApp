using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MultiTenantApp.Domain.Entities
{
    /// <summary>
    /// Represents a request/response log entry stored in MongoDB.
    /// Tracks HTTP requests and responses for debugging and auditing purposes.
    /// </summary>
    public class RequestResponseLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Tenant ID for multi-tenant isolation
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public Guid? TenantId { get; set; }

        /// <summary>
        /// User ID who made the request (if authenticated)
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public Guid? UserId { get; set; }

        /// <summary>
        /// Username who made the request (denormalized for performance)
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// HTTP method (GET, POST, PUT, DELETE, etc.)
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Request path
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Query string
        /// </summary>
        public string? QueryString { get; set; }

        /// <summary>
        /// Request headers (sanitized - sensitive headers removed)
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; set; } = new();

        /// <summary>
        /// Request body (truncated if too long)
        /// </summary>
        public string? RequestBody { get; set; }

        /// <summary>
        /// Response status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response headers (sanitized)
        /// </summary>
        public Dictionary<string, string> ResponseHeaders { get; set; } = new();

        /// <summary>
        /// Response body (truncated if too long)
        /// </summary>
        public string? ResponseBody { get; set; }

        /// <summary>
        /// Client IP address
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Duration of the request in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// When the request occurred
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Exception message if an error occurred
        /// </summary>
        public string? ExceptionMessage { get; set; }
    }
}
