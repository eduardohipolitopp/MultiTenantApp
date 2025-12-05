using MultiTenantApp.Web.Models.DTOs;

namespace MultiTenantApp.Web.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetMyProfileAsync();
        Task UpdateMyProfileAsync(UpdateProfileDto dto);
        Task<string> UploadAvatarAsync(Stream fileStream, string fileName);
        Task ChangePasswordAsync(ChangePasswordDto dto);
    }
}
