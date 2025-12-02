using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ITenantProvider _tenantProvider;
        private readonly IRepository<Tenant> _tenantRepository;

        public AuthService(UserManager<ApplicationUser> userManager,
                           Microsoft.Extensions.Configuration.IConfiguration configuration,
                           ITenantProvider tenantProvider,
                           IRepository<Tenant> tenantRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _tenantProvider = tenantProvider;
            _tenantRepository = tenantRepository;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto model)
        {
            // 1. Find Tenant by Identifier
            var tenant = await _tenantRepository.GetAsync(t => t.Identifier == model.TenantId);
            if (tenant == null)
            {
                throw new Exception(AuthServiceResource.InvalidTenant);
            }

            // 2. Set Tenant Context
            _tenantProvider.SetTenantId(tenant.Id);

            // 3. Find User (now scoped to tenant if filter is active, or just find by email)
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new Exception(AuthServiceResource.InvalidCredentials);
            }

            // 4. Double check tenant (redundant if filter works, but safe)
            if (user.TenantId != tenant.Id)
            {
                throw new Exception(AuthServiceResource.InvalidTenantForUser);
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tenant_id", user.TenantId.ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };
        }

        public async Task RegisterAsync(RegisterDto model)
        {
            if (!_configuration.GetValue<bool>("EnableSelfRegistration"))
            {
                throw new Exception("Self-registration is disabled.");
            }

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                throw new Exception(AuthServiceResource.UserAlreadyExists);

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
            };
            // Look up tenant
            var tenant = await _tenantRepository.GetAsync(t => t.Identifier == model.TenantId);
            if (tenant == null) throw new Exception(AuthServiceResource.InvalidTenant);
            
            user.TenantId = tenant.Id;
            
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new Exception(AuthServiceResource.UserCreationFailed);
        }
    }
}
