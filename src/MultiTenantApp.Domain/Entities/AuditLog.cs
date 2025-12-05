using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MultiTenantApp.Domain.Entities
{
    /// <summary>
    /// Represents an audit log entry stored in MongoDB.
    /// Tracks changes to entities for compliance and debugging.
    /// </summary>
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Tenant ID for multi-tenant isolation
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public Guid TenantId { get; set; }

        /// <summary>
        /// ID of the entity that was changed
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Type of the entity (e.g., "Product", "User")
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Action performed: Create, Update, Delete
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Dictionary containing only the fields that were changed.
        /// Key: field name, Value: { "Old": oldValue, "New": newValue }
        /// For Create: only "New" values. For Delete: only "Old" values.
        /// </summary>
        [BsonElement("changes")]
        public Dictionary<string, FieldChange> Changes { get; set; } = new();

        /// <summary>
        /// ID of the user who made the change
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        /// <summary>
        /// Name of the user (denormalized for performance)
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// When the change occurred
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a change to a single field
    /// </summary>
    public class FieldChange
    {
        /// <summary>
        /// Old value (null for Create action)
        /// </summary>
        public object? OldValue { get; set; }

        /// <summary>
        /// New value (null for Delete action)
        /// </summary>
        public object? NewValue { get; set; }
    }
}
