using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Authorize(Policy = "Finance.View")]
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FinanceDto>> GetById(Guid id)
        {
            var result = await _financeService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<FinanceListDto>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _financeService.GetAllAsync(request);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<FinanceDto>> Register(CreateFinanceDto model)
        {
            var result = await _financeService.RegisterPayment(model);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<FinanceSummaryDto>> GetSummary([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var result = await _financeService.GetSummary(start, end);
            return Ok(result);
        }

        [Authorize(Policy = "Finance.CloseMonth")]
        [HttpPost("close")]
        public async Task<IActionResult> Close([FromQuery] int month, [FromQuery] int year)
        {
            var success = await _financeService.MonthlyClosing(month, year);
            if (!success) return BadRequest("Closing failed or no data found");
            return Ok();
        }
    }
}
