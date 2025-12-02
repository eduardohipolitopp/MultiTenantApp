using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Web.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<ProductDto> GetByIdAsync(Guid id);
        Task<ProductDto> CreateAsync(CreateProductDto model);
        Task DeleteAsync(Guid id);
        Task<PagedResponse<ProductDto>> GetPagedAsync(PagedRequest request);
    }
}
