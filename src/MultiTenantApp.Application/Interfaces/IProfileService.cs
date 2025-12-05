using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<string> UploadAvatarAsync(string userId, Stream fileStream, string fileName);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
