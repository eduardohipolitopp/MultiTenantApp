using System.Collections.Generic;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        LoginResponseDto GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
