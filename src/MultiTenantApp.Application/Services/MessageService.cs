using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MessageDto?> GetByIdAsync(Guid id)
        {
            var message = await _unitOfWork.Repository<Message>().Entities
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null) return null;

            return MapToDto(message);
        }

        public async Task<PagedResponse<MessageListDto>> GetAllAsync(PagedRequest request)
        {
            var query = _unitOfWork.Repository<Message>().Entities
                .Include(m => m.Patient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(m => m.Recipient.Contains(request.SearchTerm) || 
                                     m.Patient!.Name.Contains(request.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new MessageListDto
                {
                    Id = m.Id,
                    Recipient = m.Recipient,
                    Channel = m.Channel,
                    Content = m.Content,
                    SentDate = m.SentDate,
                    Status = m.Status,
                    PatientName = m.Patient!.Name
                })
                .ToListAsync();

            return new PagedResponse<MessageListDto>(messages, request.Page, request.PageSize, totalCount);
        }

        public async Task<MessageDto> CreateAsync(CreateMessageDto model)
        {
            var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(model.PatientId);
            if (patient == null) throw new KeyNotFoundException("Patient not found");

            string content = model.CustomContent ?? "";
            if (!string.IsNullOrEmpty(model.Template))
            {
                content = await ProcessTemplate(model.Template, model.PatientId, model.AppointmentId);
            }

            var message = new Message
            {
                PatientId = model.PatientId,
                Recipient = model.Channel == MessageChannel.Email ? (patient.Email ?? "") : patient.Phone,
                Channel = model.Channel,
                Template = model.Template,
                Content = content,
                Status = MessageStatus.Pending
            };

            await _unitOfWork.Repository<Message>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(message.Id) ?? throw new Exception("Error creating message");
        }

        public async Task<bool> SendAsync(Guid messageId)
        {
            var message = await _unitOfWork.Repository<Message>().GetByIdAsync(messageId);
            if (message == null) return false;

            // In a real scenario, this would call an external SMS/WhatsApp/Email gateway
            // For this implementation, we'll simulate the send operation
            try
            {
                // Simulate delay
                await Task.Delay(500);
                
                message.Status = MessageStatus.Sent;
                message.SentDate = DateTime.UtcNow;
                
                await _unitOfWork.Repository<Message>().UpdateAsync(message);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                message.Status = MessageStatus.Failed;
                await _unitOfWork.Repository<Message>().UpdateAsync(message);
                await _unitOfWork.SaveChangesAsync();
                return false;
            }
        }

        public async Task<bool> RetryAsync(Guid messageId)
        {
            return await SendAsync(messageId);
        }

        private async Task<string> ProcessTemplate(string template, Guid patientId, Guid? appointmentId)
        {
            var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(patientId);
            var content = template;

            content = content.Replace("{PatientName}", patient?.Name ?? "Patient");

            if (appointmentId.HasValue)
            {
                var appointment = await _unitOfWork.Repository<Appointment>().Entities
                    .Include(a => a.Vaccine)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId.Value);

                if (appointment != null)
                {
                    content = content.Replace("{Vaccine}", appointment.Vaccine?.Name ?? "");
                    content = content.Replace("{ScheduledDate}", appointment.ScheduledDateTime.ToString("d"));
                    content = content.Replace("{DoseNumber}", "1"); // Simplified for now
                }
            }

            return content;
        }

        private MessageDto MapToDto(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                Recipient = message.Recipient,
                Channel = message.Channel,
                Template = message.Template,
                Content = message.Content,
                SentDate = message.SentDate,
                Status = message.Status,
                PatientId = message.PatientId,
                PatientName = message.Patient?.Name
            };
        }
    }
}
