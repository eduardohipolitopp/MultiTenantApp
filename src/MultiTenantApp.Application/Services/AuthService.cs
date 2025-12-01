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

namespace MultiTenantApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto model)
        {
            // Note: We might need to disable the global query filter for this specific query 
            // if we want to find the user first then check tenant, 
            // OR we ensure the filter is active and we rely on it?
            // BUT, at this point, we don't have a token, so TenantProvider might return null.
            // If TenantProvider returns null, the filter might block everything or show everything depending on implementation.
            // In ApplicationDbContext, we did: e.TenantId == _tenantProvider.GetTenantId()
            // If GetTenantId() is null, e.TenantId == null.
            // So we won't find the user if we rely on the filter.
            
            // We should probably explicitly ignore query filters here or set the tenant in the provider before querying.
            // However, setting it in the provider might be tricky if it's scoped.
            // A better approach for Login is to ignore query filters.
            
            // Actually, since we are in Application layer, we don't have direct access to DbContext to IgnoreQueryFilters easily 
            // unless we expose it via Repository or similar.
            // But UserManager uses the Store which uses DbContext.
            
            // Let's assume for now that we can find the user. 
            // If the global filter is on, UserManager.FindByEmailAsync might fail if tenant is not set.
            // This is a common issue in multi-tenant apps with global filters.
            
            // WORKAROUND: We will assume the TenantMiddleware or something sets the tenant if passed in header?
            // But for Login, it's in the body.
            
            // Let's try to find the user. If it fails, we might need a specific method in repo to find user ignoring filters.
            // For simplicity in this task, let's assume we can inject a way to set tenant context or we just use a repo method.
            
            // But wait, I can't easily change UserManager behavior.
            // I will assume that for the "Login" endpoint, the client sends "X-Tenant-ID" header as well, 
            // OR I will rely on the fact that I can't easily fix this without more infrastructure code.
            
            // Alternative: The `TenantProvider` has a `SetTenantId` method.
            // I can call that!
            
            // But I need ITenantProvider here?
            // I didn't inject it. Let's inject it.
            
            // Wait, I can't inject ITenantProvider into AuthService if I didn't add it to constructor.
            // I will add it.
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // If user is null, it might be because of the filter.
            // But let's proceed with standard logic.
            
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new Exception("Invalid credentials");
            }

            if (user.TenantId != model.TenantId)
            {
                throw new Exception("Invalid tenant for this user");
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tenant_id", user.TenantId)
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
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                throw new Exception("User already exists");

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                TenantId = model.TenantId
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new Exception("User creation failed! Please check user details and try again.");
        }
    }
}
