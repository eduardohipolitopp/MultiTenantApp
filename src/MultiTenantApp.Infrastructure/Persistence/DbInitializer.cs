using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // Tenants
            if (!await context.Tenants.AnyAsync(t => t.Identifier == "tenant-a"))
            {
                await context.Tenants.AddAsync(new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Tenant A",
                    Identifier = "tenant-a",
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!await context.Tenants.AnyAsync(t => t.Identifier == "tenant-b"))
            {
                await context.Tenants.AddAsync(new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Tenant B",
                    Identifier = "tenant-b",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();

            // Users
            // Tenant A
            if (await userManager.FindByEmailAsync("admin@tenant-a.com") == null)
            {
                var tenantA = await context.Tenants.FirstAsync(t => t.Identifier == "tenant-a");
                var user = new ApplicationUser
                {
                    UserName = "admin@tenant-a.com",
                    Email = "admin@tenant-a.com",
                    TenantId = tenantA.Id,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "Password123!");
                await userManager.AddToRoleAsync(user, "Admin");
            }

            // Tenant B
            if (await userManager.FindByEmailAsync("admin@tenant-b.com") == null)
            {
                var tenantB = await context.Tenants.FirstAsync(t => t.Identifier == "tenant-b");
                var user = new ApplicationUser
                {
                    UserName = "admin@tenant-b.com",
                    Email = "admin@tenant-b.com",
                    TenantId = tenantB.Id,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "Password123!");
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
