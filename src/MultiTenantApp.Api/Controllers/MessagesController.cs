using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDto>> GetById(Guid id)
        {
            var result = await _messageService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<MessageListDto>>> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _messageService.GetAllAsync(request);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> Create(CreateMessageDto model)
        {
            var result = await _messageService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("{id}/send")]
        public async Task<IActionResult> Send(Guid id)
        {
            var success = await _messageService.SendAsync(id);
            if (!success) return BadRequest("Error sending message");
            return Ok();
        }

        [HttpPost("{id}/retry")]
        public async Task<IActionResult> Retry(Guid id)
        {
            var success = await _messageService.RetryAsync(id);
            if (!success) return BadRequest("Error retrying message");
            return Ok();
        }
    }
}
