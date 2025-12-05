using MultiTenantApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Domain.Interfaces
{
    /// <summary>
    /// Repository for querying audit logs from MongoDB
    /// </summary>
    public interface IAuditRepository
    {
        /// <summary>
        /// Gets audit history for a specific entity
        /// </summary>
        Task<List<AuditLog>> GetEntityHistoryAsync(Guid entityId, string entityType, Guid tenantId);

        /// <summary>
        /// Gets paginated audit logs with filters
        /// </summary>
        Task<(List<AuditLog> Items, long TotalCount)> GetAuditLogsAsync(
            Guid tenantId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Guid? userId = null,
            string? entityType = null,
            int page = 1,
            int pageSize = 20);
    }
}
