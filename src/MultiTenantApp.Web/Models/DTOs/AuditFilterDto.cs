using System;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class AuditFilterDto : PagedRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? UserId { get; set; }
        public string? EntityType { get; set; }
    }
}
