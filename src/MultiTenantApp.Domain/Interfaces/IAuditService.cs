using MultiTenantApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Domain.Interfaces
{
    /// <summary>
    /// Service for creating audit logs in MongoDB
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Creates an audit log entry
        /// </summary>
        Task CreateAuditLogAsync(AuditLog auditLog);

        /// <summary>
        /// Creates multiple audit log entries in a batch
        /// </summary>
        Task CreateAuditLogsBatchAsync(IEnumerable<AuditLog> auditLogs);
    }
}
