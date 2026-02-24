using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Api.Attributes;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Authorize(Policy = "Appointment.View")]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetById(Guid id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        [HttpGet]
        [Cached(60)]
        public async Task<ActionResult<PagedResponse<AppointmentListDto>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _appointmentService.GetAllAsync(request);
            return Ok(result);
        }

        [HttpGet("calendar")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var result = await _appointmentService.GetByDateRangeAsync(start, end);
            return Ok(result);
        }

        [Authorize(Policy = "Appointment.Create")]
        [Idempotent]
        [InvalidateCache("api/appointments")]
        public async Task<ActionResult<AppointmentDto>> Create(CreateAppointmentDto model)
        {
            var result = await _appointmentService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Policy = "Appointment.Edit")]
        [InvalidateCache("api/appointments")]
        public async Task<IActionResult> Update(Guid id, UpdateAppointmentDto model)
        {
            await _appointmentService.UpdateAsync(id, model);
            return NoContent();
        }

        [Authorize(Policy = "Appointment.Cancel")]
        [InvalidateCache("api/appointments")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _appointmentService.DeleteAsync(id);
            return NoContent();
        }
    }
}
