using System;
using System.Threading.Tasks;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Resources;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Application.Services.Tenants
{
    public class TenantValidationService : ITenantValidationService
    {
        private readonly IRepository<Tenant> _tenantRepository;

        public TenantValidationService(IRepository<Tenant> tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<Tenant> ValidateAndGetTenantAsync(string identifier)
        {
            var tenant = await _tenantRepository.GetAsync(t => t.Identifier == identifier);
            if (tenant == null)
            {
                throw new Exception(AuthServiceResource.InvalidTenant);
            }
            return tenant;
        }
    }
}
