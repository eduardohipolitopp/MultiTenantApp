using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Application.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorageService;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorageService)
        {
            _userManager = userManager;
            _fileStorageService = fileStorageService;
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                FullName = user.FullName,
                Age = user.Age,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                PreferredLanguage = user.PreferredLanguage,
                EmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            user.FullName = dto.FullName;
            user.Age = dto.Age;
            user.Address = dto.Address;
            user.PhoneNumber = dto.PhoneNumber;
            user.PreferredLanguage = dto.PreferredLanguage;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update profile: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task<string> UploadAvatarAsync(string userId, Stream fileStream, string fileName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                await _fileStorageService.DeleteAvatarAsync(user.AvatarUrl);
            }

            // Upload new avatar
            var avatarFileName = await _fileStorageService.UploadAvatarAsync(fileStream, fileName, userId);

            // Update user
            user.AvatarUrl = avatarFileName;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update avatar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return avatarFileName;
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to change password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
