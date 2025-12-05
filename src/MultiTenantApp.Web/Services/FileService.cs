using MultiTenantApp.Web.Interfaces;
using System.Net;

namespace MultiTenantApp.Web.Services
{
    public class FileService : IFileService
    {
        private readonly AuthenticatedHttpClient _httpClient;

        public FileService(AuthenticatedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetFileAsBase64Async(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            try
            {
                // URL encode the file path to handle special characters
                var encodedPath = WebUtility.UrlEncode(filePath);
                var response = await _httpClient.GetAsync($"api/Files/{encodedPath}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Read the file content as bytes
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var base64 = Convert.ToBase64String(fileBytes);

                // Get content type from response or infer from file extension
                var contentType = response.Content.Headers.ContentType?.MediaType ?? GetContentTypeFromExtension(filePath);

                // Return as data URL
                return $"data:{contentType};base64,{base64}";
            }
            catch
            {
                // Return null if any error occurs (file not found, network error, etc.)
                return null;
            }
        }

        private static string GetContentTypeFromExtension(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
