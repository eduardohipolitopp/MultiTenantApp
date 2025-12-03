using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Web.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(Guid id);
        Task<ProductDto> CreateAsync(CreateProductDto model);
        Task UpdateAsync(Guid id, UpdateProductDto model);
        Task DeleteAsync(Guid id);
        Task<PagedResponse<ProductDto>> GetPagedAsync(PagedRequest request);
    }
}
