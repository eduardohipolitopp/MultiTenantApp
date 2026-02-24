using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantApp.Domain.Entities
{
    [LogicalDelete]
    public class Message : BaseTenantEntity
    {
        public string Recipient { get; set; } = string.Empty;
        public MessageChannel Channel { get; set; }
        public string? Template { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Pending;

        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }
        public Patient? Patient { get; set; }
    }
}
