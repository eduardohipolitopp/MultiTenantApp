using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<ProductDto>> GetAllAsync(PagedRequest request)
        {
            System.Linq.Expressions.Expression<Func<Product, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                filter = p => p.Name.Contains(request.SearchTerm) || p.Description.Contains(request.SearchTerm);
            }

            var (products, totalCount) = await _unitOfWork.Repository<Product>().GetPagedAsync(request.Page, request.PageSize, filter);
            
            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price
            }).ToList();

            return new PagedResponse<ProductDto>(productDtos, request.Page, request.PageSize, totalCount);
        }

        public async Task<ProductDto> GetByIdAsync(Guid id)
        {
            var p = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (p == null) return null;

            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price
            };
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto model)
        {
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price
            };

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };
        }

        public async Task UpdateAsync(Guid id, UpdateProductDto model)
        {
            var repository = _unitOfWork.Repository<Product>();
            var product = await repository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;

            await repository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var repository = _unitOfWork.Repository<Product>();
            var product = await repository.GetByIdAsync(id);
            if (product != null)
            {
                await repository.DeleteAsync(product);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
