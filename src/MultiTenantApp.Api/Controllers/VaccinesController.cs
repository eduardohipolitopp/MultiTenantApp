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
    [Authorize(Policy = "Vaccine.View")]
    [ApiController]
    [Route("api/[controller]")]
    public class VaccinesController : ControllerBase
    {
        private readonly IVaccineService _vaccineService;

        public VaccinesController(IVaccineService vaccineService)
        {
            _vaccineService = vaccineService;
        }

        [HttpGet("details/{id}")]
        [Cached(durationMinutes: 10)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _vaccineService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("create")]
        [Authorize(Policy = "Vaccine.Create")]
        [Idempotent]
        [LogRequestResponse]
        [InvalidateCache("api/Vaccines/list")]
        public async Task<IActionResult> Create([FromBody] CreateVaccineDto model)
        {
            var result = await _vaccineService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("update/{id}")]
        [Authorize(Policy = "Vaccine.Edit")]
        [LogRequestResponse]
        [InvalidateCache("api/Vaccines/list")]
        [InvalidateCache("api/Vaccines/details/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVaccineDto model)
        {
            await _vaccineService.UpdateAsync(id, model);
            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "Vaccine.Edit")]
        [LogRequestResponse]
        [InvalidateCache("api/Vaccines/list")]
        [InvalidateCache("api/Vaccines/details/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _vaccineService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("list")]
        [Cached(durationMinutes: 5, varyByTenant: true)]
        public async Task<IActionResult> GetAll([FromBody] PagedRequest request)
        {
            var result = await _vaccineService.GetAllAsync(request);
            return Ok(result);
        }
    }
}
