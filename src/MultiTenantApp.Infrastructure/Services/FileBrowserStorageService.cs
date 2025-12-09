using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Helpers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MultiTenantApp.Infrastructure.Services
{
    public class FileBrowserStorageService : IFileStorageService
    {
        private readonly string _fileBrowserUrl;
        private readonly string _fileBrowserUsername;
        private readonly string _fileBrowserPassword;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileBrowserStorageService> _logger;

        public FileBrowserStorageService(IConfiguration configuration, ILogger<FileBrowserStorageService> logger)
        {
            _fileBrowserUrl = configuration["FileStorage:FileBrowserUrl"] ?? "http://filebrowser:80";
            _fileBrowserUsername = configuration["FileStorage:FileBrowserUsername"] ?? "admin";
            _fileBrowserPassword = configuration["FileStorage:FileBrowserPassword"] ?? "admin";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_fileBrowserUrl)
            };

            var credentialBytes = Encoding.UTF8.GetBytes($"{_fileBrowserUsername}:{_fileBrowserPassword}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentialBytes));

            _logger = logger;
        }

        public async Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string userId)
        {
            return await UploadFileAsync(fileStream, fileName, FileCategory.Avatar, userId);
        }

        public async Task<string> SaveReportAsync(Stream fileStream, string fileName, string userId)
        {
            return await UploadFileAsync(fileStream, fileName, FileCategory.Report, userId);
        }

        public async Task<string> SaveExportAsync(Stream fileStream, string fileName, string userId)
        {
            return await UploadFileAsync(fileStream, fileName, FileCategory.Export, userId);
        }

        public async Task<string> SaveImportAsync(Stream fileStream, string fileName, string userId)
        {
            return await UploadFileAsync(fileStream, fileName, FileCategory.Import, userId);
        }

        public async Task<string> SaveDocumentAsync(Stream fileStream, string fileName, string userId)
        {
            return await UploadFileAsync(fileStream, fileName, FileCategory.Document, userId);
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, FileCategory category, string userId)
        {
            return await UploadFileAsync(fileStream, fileName, category, userId);
        }

        private async Task<string> UploadFileAsync(Stream fileStream, string fileName, FileCategory category, string userId)
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

                // Generate unique filename
                var uniqueFileName = FileStorageHelper.GenerateUniqueFileName(fileName, userId);

                // Create upload path
                var uploadPath = $"/files/{categoryPath}";

                // Ensure directory exists in filebrowser
                await EnsureDirectoryExistsAsync(uploadPath);

                // Upload file to filebrowser
                var fileUrl = await UploadToFileBrowserAsync(fileStream, uniqueFileName, uploadPath);

                _logger.LogInformation("File uploaded to filebrowser: {Category}/{FileName} for user {UserId}",
                    categoryPath, uniqueFileName, userId);

                return $"{categoryPath}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to filebrowser for user {UserId}", userId);
                throw;
            }
        }

        private async Task EnsureDirectoryExistsAsync(string path)
        {
            try
            {
                // Try to create directory - filebrowser API might not need this
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/resources")
                {
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        action = "mkdir",
                        items = new[] { new { from = "", to = path, name = Path.GetFileName(path) } }
                    }), new MediaTypeHeaderValue("application/json"))
                };

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Could not create directory {Path}: {StatusCode}", path, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error ensuring directory exists: {Path}", path);
            }
        }

        private async Task<string> UploadToFileBrowserAsync(Stream fileStream, string fileName, string uploadPath)
        {
            using var content = new MultipartFormDataContent();

            // Reset stream position
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", fileName);

            // Add override parameter to prevent filename conflicts
            content.Add(new StringContent("true"), "override");

            var uploadUrl = $"/api/resources{uploadPath}?action=upload";
            var response = await _httpClient.PostAsync(uploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Filebrowser upload failed: {response.StatusCode} - {errorContent}");
            }

            return $"{_fileBrowserUrl}/files{uploadPath}/{fileName}";
        }

        public async Task<Stream> GetFileAsync(string filePath)
        {
            var fullUrl = $"{_fileBrowserUrl}/files/{filePath}";
            var response = await _httpClient.GetAsync(fullUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task DeleteFileAsync(string filePath)
        {
            try
            {
                var deleteUrl = $"/api/resources/files/{filePath}";
                var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("File deleted from filebrowser: {FilePath}", filePath);
                }
                else
                {
                    _logger.LogWarning("Failed to delete file from filebrowser: {FilePath} - {StatusCode}",
                        filePath, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from filebrowser: {FilePath}", filePath);
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
            return $"{_fileBrowserUrl}/files/{categoryPath}/{fileName}";
        }
    }
}