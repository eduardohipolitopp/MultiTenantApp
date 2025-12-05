using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ITenantProvider _tenantProvider;

        public AuditController(IAuditRepository auditRepository, ITenantProvider tenantProvider)
        {
            _auditRepository = auditRepository;
            _tenantProvider = tenantProvider;
        }

        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<IActionResult> GetEntityHistory(string entityType, Guid entityId)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == null) return Unauthorized();

            var logs = await _auditRepository.GetEntityHistoryAsync(entityId, entityType, tenantId.Value);
            
            var dtos = logs.Select(log => new AuditLogDto
            {
                Id = log.Id,
                EntityId = log.EntityId,
                EntityType = log.EntityType,
                Action = log.Action,
                UserId = log.UserId,
                UserName = log.UserName,
                Timestamp = log.Timestamp,
                Changes = log.Changes.ToDictionary(
                    k => k.Key, 
                    v => new FieldChangeDto { OldValue = v.Value.OldValue, NewValue = v.Value.NewValue })
            });

            return Ok(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditFilterDto filter)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == null) return Unauthorized();

            var (items, totalCount) = await _auditRepository.GetAuditLogsAsync(
                tenantId.Value,
                filter.StartDate,
                filter.EndDate,
                filter.UserId,
                filter.EntityType,
                filter.Page,
                filter.PageSize
            );

            var dtos = items.Select(log => new AuditLogDto
            {
                Id = log.Id,
                EntityId = log.EntityId,
                EntityType = log.EntityType,
                Action = log.Action,
                UserId = log.UserId,
                UserName = log.UserName,
                Timestamp = log.Timestamp,
                Changes = log.Changes.ToDictionary(
                    k => k.Key,
                    v => new FieldChangeDto { OldValue = v.Value.OldValue, NewValue = v.Value.NewValue })
            }).ToList();

            return Ok(new PagedResponse<AuditLogDto>(dtos, filter.Page, filter.PageSize, (int)totalCount));
        }
    }
}
