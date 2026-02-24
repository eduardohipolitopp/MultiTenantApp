using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Attributes;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Authorize(Policy = "Stock.View")]
    [ApiController]
    [Route("api/[controller]")]
    public class VaccineBatchesController : ControllerBase
    {
        private readonly IVaccineBatchService _batchService;

        public VaccineBatchesController(IVaccineBatchService batchService)
        {
            _batchService = batchService;
        }

        [HttpGet("details/{id}")]
        [Cached(durationMinutes: 10)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _batchService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("create")]
        [Authorize(Policy = "Stock.Create")]
        [Idempotent]
        [LogRequestResponse]
        [InvalidateCache("api/VaccineBatches/list")]
        public async Task<IActionResult> Create([FromBody] CreateVaccineBatchDto model)
        {
            var result = await _batchService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("update/{id}")]
        [Authorize(Policy = "Stock.Edit")]
        [LogRequestResponse]
        [InvalidateCache("api/VaccineBatches/list")]
        [InvalidateCache("api/VaccineBatches/details/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVaccineBatchDto model)
        {
            await _batchService.UpdateAsync(id, model);
            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "Stock.Edit")]
        [LogRequestResponse]
        [InvalidateCache("api/VaccineBatches/list")]
        [InvalidateCache("api/VaccineBatches/details/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _batchService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("list")]
        [Cached(durationMinutes: 5, varyByTenant: true)]
        public async Task<IActionResult> GetAll([FromBody] PagedRequest request)
        {
            var result = await _batchService.GetAllAsync(request);
            return Ok(result);
        }

        [HttpGet("next-fifo/{vaccineId}")]
        public async Task<IActionResult> GetNextFIFO(Guid vaccineId)
        {
            var result = await _batchService.GetNextAvailableBatchFIFO(vaccineId);
            if (result == null) return NotFound("No available batches for this vaccine.");
            return Ok(result);
        }
    }
}
