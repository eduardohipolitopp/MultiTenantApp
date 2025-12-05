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
            // Cria um scope para resolver serviços scoped corretamente
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var context = provider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            // TENANTS
            var tenatnA = await context.Tenants.FirstOrDefaultAsync(t => t.Identifier == "tenant-a");
            if (tenatnA == null)
            {
                tenatnA = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Tenant A",
                    Identifier = "tenant-a",
                    CreatedAt = DateTime.UtcNow
                };
                await context.Tenants.AddAsync(tenatnA);
            }

            var tenatnB = await context.Tenants.FirstOrDefaultAsync(t => t.Identifier == "tenant-b");
            if (tenatnB == null)
            {
                tenatnB = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Tenant B",
                    Identifier = "tenant-b",
                    CreatedAt = DateTime.UtcNow
                };
                await context.Tenants.AddAsync(tenatnB);
            }

            // RULE
            var adminRule = await context.Rules.FirstOrDefaultAsync(t => t.Name == "Admin");
            if (adminRule == null)
            {
                adminRule = new Rule
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "General Administrator",
                    CreatedAt = DateTime.UtcNow
                };
                await context.Rules.AddAsync(adminRule);
            }

            // Persistir tenants & rule antes de criar usuários (evita dependências entre contextos)
            await context.SaveChangesAsync();

            // USER A
            if (await userManager.FindByNameAsync("a.admin@admin.com") == null)
            {
                var user = new ApplicationUser
                {
                    FullName = "Administrator Tenant A",
                    UserName = "a.admin@admin.com",
                    Email = "a.admin@admin.com",
                    TenantId = tenatnA.Id,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "123Mudar!");
                if (!result.Succeeded)
                {
                    // logue ou lance exceção — importante para descobrir o motivo
                    var errors = string.Join("; ", result.Errors);
                    throw new Exception($"Falha ao criar usuário a.admin@admin.com: {errors}");
                }

                await context.UserRules.AddAsync(new UserRule
                {
                    Id = Guid.NewGuid(),
                    RuleId = adminRule.Id,
                    UserId = user.Id,
                    PermissionType = MultiTenantApp.Domain.Enums.PermissionType.Edit,
                    TenantId = tenatnA.Id
                });

                await context.SaveChangesAsync();
            }

            // USER B
            if (await userManager.FindByNameAsync("b.admin@admin.com") == null)
            {
                var user = new ApplicationUser
                {
                    FullName = "Administrator Tenant B",
                    UserName = "b.admin@admin.com",
                    Email = "b.admin@admin.com",
                    TenantId = tenatnB.Id,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "123Mudar!");
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors);
                    throw new Exception($"Falha ao criar usuário b.admin@admin.com: {errors}");
                }

                await context.UserRules.AddAsync(new UserRule
                {
                    Id = Guid.NewGuid(),
                    RuleId = adminRule.Id,
                    UserId = user.Id,
                    PermissionType = MultiTenantApp.Domain.Enums.PermissionType.Edit,
                    // corrigido: TenantId deve ser do tenant B
                    TenantId = tenatnB.Id
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
