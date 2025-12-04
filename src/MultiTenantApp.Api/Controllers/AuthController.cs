using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;

namespace MultiTenantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserRegistrationService _userRegistrationService;

        public AuthController(
            IAuthenticationService authenticationService,
            IUserRegistrationService userRegistrationService)
        {
            _authenticationService = authenticationService;
            _userRegistrationService = userRegistrationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var result = await _authenticationService.AuthenticateAsync(model);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                await _userRegistrationService.RegisterAsync(model);
                return Ok(new { message = "User created successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
