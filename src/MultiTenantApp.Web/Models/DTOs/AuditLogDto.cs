using System;
using System.Collections.Generic;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, FieldChangeDto> Changes { get; set; } = new();
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class FieldChangeDto
    {
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}
