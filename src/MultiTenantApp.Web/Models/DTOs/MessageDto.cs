using MultiTenantApp.Web.Models.DTOs;

namespace MultiTenantApp.Web.Models.DTOs
{
    public enum MessageChannel
    {
        SMS,
        WhatsApp,
        Email
    }

    public enum MessageStatus
    {
        Pending,
        Sent,
        Failed
    }

    public class MessageDto
    {
        public Guid Id { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public MessageChannel Channel { get; set; }
        public string? Template { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        public MessageStatus Status { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
    }

    public class CreateMessageDto
    {
        public Guid PatientId { get; set; }
        public MessageChannel Channel { get; set; }
        public string? Template { get; set; }
        public string? CustomContent { get; set; }
        public Guid? AppointmentId { get; set; }
    }

    public class MessageListDto
    {
        public Guid Id { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public MessageChannel Channel { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        public MessageStatus Status { get; set; }
        public string? PatientName { get; set; }
    }
}
