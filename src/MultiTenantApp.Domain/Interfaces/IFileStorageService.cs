using MultiTenantApp.Domain.Enums;

namespace MultiTenantApp.Domain.Interfaces
{
    public interface IFileStorageService
    {
        // Avatar operations
        Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string userId);
        Task DeleteAvatarAsync(string avatarUrl);
        
        // Report operations
        Task<string> SaveReportAsync(Stream fileStream, string fileName, string userId);
        
        // Export operations
        Task<string> SaveExportAsync(Stream fileStream, string fileName, string userId);
        
        // Import operations
        Task<string> SaveImportAsync(Stream fileStream, string fileName, string userId);
        
        // Document operations
        Task<string> SaveDocumentAsync(Stream fileStream, string fileName, string userId);
        
        // Generic operations
        Task<string> SaveFileAsync(Stream fileStream, string fileName, FileCategory category, string userId);
        Task<Stream> GetFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        string GetFileUrl(string fileName, FileCategory category);
    }
}
