using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Resources;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Application.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantValidationService _tenantValidationService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ITenantProvider _tenantProvider;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            ITenantValidationService tenantValidationService,
            IJwtTokenGenerator jwtTokenGenerator,
            ITenantProvider tenantProvider)
        {
            _userManager = userManager;
            _tenantValidationService = tenantValidationService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _tenantProvider = tenantProvider;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto model)
        {
            // 1. Validate Tenant
            var tenant = await _tenantValidationService.ValidateAndGetTenantAsync(model.TenantId);

            // 2. Set Tenant Context (Important for filtering if using global filters)
            _tenantProvider.SetTenantId(tenant.Id);

            // 3. Find User
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new Exception(AuthServiceResource.InvalidCredentials);
            }

            // 4. Double check tenant
            if (user.TenantId != tenant.Id)
            {
                throw new Exception(AuthServiceResource.InvalidTenantForUser);
            }

            // 5. Generate Token
            var roles = await _userManager.GetRolesAsync(user);
            return _jwtTokenGenerator.GenerateToken(user, roles);
        }
    }
}
