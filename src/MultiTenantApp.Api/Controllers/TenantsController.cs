using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Infrastructure.Persistence;

namespace MultiTenantApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantsController> _logger;

        public TenantsController(ApplicationDbContext context, ILogger<TenantsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all tenants (for login dropdown)
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<ActionResult<List<TenantDto>>> GetTenantsList()
        {
            try
            {
                var tenants = await _context.Tenants
                    .Select(t => new TenantDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Identifier = t.Identifier,
                        CreatedAt = t.CreatedAt
                    })
                    .ToListAsync();

                return Ok(tenants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenants list");
                return StatusCode(500, "Error retrieving tenants");
            }
        }

        /// <summary>
        /// Get tenant by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TenantDto>> GetTenant(Guid id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                
                if (tenant == null)
                {
                    return NotFound();
                }

                var tenantDto = new TenantDto
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Identifier = tenant.Identifier,
                    CreatedAt = tenant.CreatedAt
                };

                return Ok(tenantDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant {TenantId}", id);
                return StatusCode(500, "Error retrieving tenant");
            }
        }
    }
}
