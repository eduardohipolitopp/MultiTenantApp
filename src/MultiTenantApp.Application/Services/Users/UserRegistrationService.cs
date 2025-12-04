using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Resources;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Application.Services.Users
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantValidationService _tenantValidationService;
        private readonly IConfiguration _configuration;

        public UserRegistrationService(
            UserManager<ApplicationUser> userManager,
            ITenantValidationService tenantValidationService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tenantValidationService = tenantValidationService;
            _configuration = configuration;
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

            // Validate Tenant
            var tenant = await _tenantValidationService.ValidateAndGetTenantAsync(model.TenantId);

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                TenantId = tenant.Id
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new Exception(AuthServiceResource.UserCreationFailed);
        }
    }
}
