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
    [Authorize(Policy = "Patient.View")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // POST api/patients/list
        [HttpPost("list")]
        [Cached(durationMinutes: 5, varyByTenant: true)]
        public async Task<IActionResult> GetAll([FromBody] PagedRequest request)
        {
            var response = await _patientService.GetAllAsync(request);
            return Ok(response);
        }

        // GET api/patients/details/{id}
        [HttpGet("details/{id}")]
        [Cached(durationMinutes: 5, varyByTenant: true)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(patient);
        }

        // POST api/patients/create
        [HttpPost("create")]
        [Authorize(Policy = "Patient.Create")]
        [Idempotent]
        [InvalidateCache("action:Patients:*")]
        [LogRequestResponse]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto model)
        {
            var patient = await _patientService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
        }

        // PUT api/patients/update/{id}
        [HttpPut("update/{id}")]
        [Authorize(Policy = "Patient.Edit")]
        [InvalidateCache("action:Patients:*")]
        [LogRequestResponse]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientDto model)
        {
            await _patientService.UpdateAsync(id, model);
            return NoContent();
        }

        // DELETE api/patients/delete/{id}
        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "Patient.Delete")]
        [Idempotent]
        [InvalidateCache("action:Patients:*")]
        [LogRequestResponse]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _patientService.DeleteAsync(id);
            return NoContent();
        }
    }
}
