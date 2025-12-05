using MultiTenantApp.Web.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Services
{
    public interface IAuditService
    {
        Task<List<AuditLogDto>> GetEntityHistoryAsync(string entityType, Guid entityId);
        Task<PagedResponse<AuditLogDto>> GetAuditLogsAsync(AuditFilterDto filter);
    }

    public class AuditService : IAuditService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public AuditService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<AuditLogDto>> GetEntityHistoryAsync(string entityType, Guid entityId)
        {
            return await _httpClient.GetFromJsonAsync<List<AuditLogDto>>($"api/audit/entity/{entityType}/{entityId}") ?? new List<AuditLogDto>();
        }

        public async Task<PagedResponse<AuditLogDto>> GetAuditLogsAsync(AuditFilterDto filter)
        {
            var queryParams = new List<string>
            {
                $"page={filter.Page}",
                $"pageSize={filter.PageSize}"
            };

            if (filter.StartDate.HasValue) queryParams.Add($"startDate={filter.StartDate.Value:O}");
            if (filter.EndDate.HasValue) queryParams.Add($"endDate={filter.EndDate.Value:O}");
            if (filter.UserId.HasValue) queryParams.Add($"userId={filter.UserId}");
            if (!string.IsNullOrEmpty(filter.EntityType)) queryParams.Add($"entityType={filter.EntityType}");

            var queryString = string.Join("&", queryParams);
            return await _httpClient.GetFromJsonAsync<PagedResponse<AuditLogDto>>($"api/audit?{queryString}") 
                   ?? new PagedResponse<AuditLogDto>(new List<AuditLogDto>(), filter.Page, filter.PageSize, 0);
        }
    }
}
