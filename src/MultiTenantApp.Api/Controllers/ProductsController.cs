using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Enums;

namespace MultiTenantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequirePermission("Products", PermissionType.View)]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // POST api/products/list
        [HttpPost("list")]
        [Cached(durationMinutes: 10, varyByTenant: true)]
        public async Task<IActionResult> GetAll([FromBody] PagedRequest request)
        {
            var response = await _productService.GetAllAsync(request);
            return Ok(response);
        }

        // GET api/products/details/{id}
        [HttpGet("details/{id}")]
        [Cached(durationMinutes: 30, varyByTenant: true)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        /// <summary>
        /// Creates a new product.
        /// Request/Response is logged to MongoDB for auditing purposes.
        /// </summary>
        /// <param name="model">Product creation data</param>
        /// <returns>Created product</returns>
        [HttpPost("create")]
        [RequirePermission("Products", PermissionType.Edit)]
        [Idempotent]
        [InvalidateCache("action:Products:*")]
        [LogRequestResponse]
        public async Task<IActionResult> Create([FromBody] CreateProductDto model)
        {
            var product = await _productService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        // PUT api/products/update/{id}
        [HttpPut("update/{id}")]
        [RequirePermission("Products", PermissionType.Edit)]
        [InvalidateCache("action:Products:*")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto model)
        {
            await _productService.UpdateAsync(id, model);
            return NoContent();
        }

        // DELETE api/products/delete/{id}
        [HttpDelete("delete/{id}")]
        [RequirePermission("Products", PermissionType.Edit)]
        [Idempotent]
        [InvalidateCache("action:Products:*")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
    }
}
