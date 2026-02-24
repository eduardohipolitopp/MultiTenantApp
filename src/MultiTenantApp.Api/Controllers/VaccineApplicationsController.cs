using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Authorize(Policy = "Patient.View")]
    [ApiController]
    [Route("api/[controller]")]
    public class VaccineApplicationsController : ControllerBase
    {
        private readonly IVaccineApplicationService _vaccineApplicationService;

        public VaccineApplicationsController(IVaccineApplicationService vaccineApplicationService)
        {
            _vaccineApplicationService = vaccineApplicationService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VaccineApplicationDto>> GetById(Guid id)
        {
            var result = await _vaccineApplicationService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<VaccineApplicationListDto>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _vaccineApplicationService.GetAllAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "Application.Apply")]
        [HttpPost("apply")]
        public async Task<ActionResult<VaccineApplicationDto>> Apply(CreateVaccineApplicationDto model)
        {
            try
            {
                var result = await _vaccineApplicationService.ApplyVaccine(model);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
