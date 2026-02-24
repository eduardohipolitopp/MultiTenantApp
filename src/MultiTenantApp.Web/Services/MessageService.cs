using MultiTenantApp.Web.Interfaces;
using MultiTenantApp.Web.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Services
{
    public class MessageService : IMessageService
    {
        private readonly HttpClient _httpClient;

        public MessageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MessageDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<MessageDto>($"api/messages/{id}");
        }

        public async Task<PagedResponse<MessageListDto>> GetAllAsync(PagedRequest request)
        {
            var queryString = $"?page={request.Page}&pageSize={request.PageSize}&searchTerm={request.SearchTerm}&sortBy={request.SortBy}&sortDescending={request.SortDescending}";
            return await _httpClient.GetFromJsonAsync<PagedResponse<MessageListDto>>($"api/messages{queryString}")
                   ?? new PagedResponse<MessageListDto>(new List<MessageListDto>(), 1, 10, 0);
        }

        public async Task<MessageDto> CreateAsync(CreateMessageDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/messages", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MessageDto>() ?? throw new Exception("Error creating message");
        }

        public async Task<bool> SendAsync(Guid messageId)
        {
            var response = await _httpClient.PostAsync($"api/messages/{messageId}/send", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RetryAsync(Guid messageId)
        {
            var response = await _httpClient.PostAsync($"api/messages/{messageId}/retry", null);
            return response.IsSuccessStatusCode;
        }
    }
}
