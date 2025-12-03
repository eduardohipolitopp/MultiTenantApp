using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedResponse<ProductDto>> GetAllAsync(PagedRequest request);
        Task<ProductDto> GetByIdAsync(Guid id);
        Task<ProductDto> CreateAsync(CreateProductDto model);
        Task UpdateAsync(Guid id, UpdateProductDto model);
        Task DeleteAsync(Guid id);
    }
}
