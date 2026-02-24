using MultiTenantApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto?> GetByIdAsync(Guid id);
        Task<PagedResponse<MessageListDto>> GetAllAsync(PagedRequest request);
        Task<MessageDto> CreateAsync(CreateMessageDto model);
        Task<bool> SendAsync(Guid messageId);
        Task<bool> RetryAsync(Guid messageId);
    }
}
