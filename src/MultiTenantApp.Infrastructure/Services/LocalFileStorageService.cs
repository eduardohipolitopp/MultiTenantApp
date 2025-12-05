using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Helpers;

namespace MultiTenantApp.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly string _fileBrowserUrl;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
        {
            _basePath = configuration["FileStorage:BasePath"] ?? "./data/files";
            _fileBrowserUrl = configuration["FileStorage:FileBrowserUrl"] ?? "http://localhost:8080";
            _logger = logger;

            // Ensure base directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string userId)
        {
            return await SaveFileAsync(fileStream, fileName, FileCategory.Avatar, userId);
        }

        public async Task<string> SaveReportAsync(Stream fileStream, string fileName, string userId)
        {
            return await SaveFileAsync(fileStream, fileName, FileCategory.Report, userId);
        }

        public async Task<string> SaveExportAsync(Stream fileStream, string fileName, string userId)
        {
            return await SaveFileAsync(fileStream, fileName, FileCategory.Export, userId);
        }

        public async Task<string> SaveImportAsync(Stream fileStream, string fileName, string userId)
        {
            return await SaveFileAsync(fileStream, fileName, FileCategory.Import, userId);
        }

        public async Task<string> SaveDocumentAsync(Stream fileStream, string fileName, string userId)
        {
            return await SaveFileAsync(fileStream, fileName, FileCategory.Document, userId);
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, FileCategory category, string userId)
        {
            try
            {
                // Validate file size
                var maxSize = FileStorageHelper.GetMaxFileSizeBytes(category);
                if (fileStream.Length > maxSize)
                {
                    throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxSize / 1024 / 1024}MB");
                }

                // Validate file type
                if (!FileStorageHelper.IsValidFileType(fileName, category))
                {
                    throw new InvalidOperationException($"File type not allowed for category {category}");
                }

                // Get category path
                var categoryPath = FileStorageHelper.GetCategoryPath(category);
                var fullCategoryPath = Path.Combine(_basePath, categoryPath);

                // Ensure category directory exists
                if (!Directory.Exists(fullCategoryPath))
                {
                    Directory.CreateDirectory(fullCategoryPath);
                }

                // Generate unique filename
                var uniqueFileName = FileStorageHelper.GenerateUniqueFileName(fileName, userId);
                var filePath = Path.Combine(fullCategoryPath, uniqueFileName);

                // Save file
                using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStreamOutput);
                }

                _logger.LogInformation("File saved successfully: {Category}/{FileName} for user {UserId}", 
                    categoryPath, uniqueFileName, userId);

                return Path.Combine(categoryPath, uniqueFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Stream> GetFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("File deleted: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
                // Don't throw - deletion failure shouldn't block other operations
            }
        }

        public async Task DeleteAvatarAsync(string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
                return;

            // Extract relative path from URL
            var fileName = avatarUrl.Replace("/files/", "");
            await DeleteFileAsync(fileName);
        }

        public string GetFileUrl(string fileName, FileCategory category)
        {
            var categoryPath = FileStorageHelper.GetCategoryPath(category);
            return $"/api/files/{categoryPath}/{fileName}";
        }
    }
}
