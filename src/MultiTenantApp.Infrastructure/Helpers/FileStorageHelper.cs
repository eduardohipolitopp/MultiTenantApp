using MultiTenantApp.Domain.Enums;

namespace MultiTenantApp.Infrastructure.Helpers
{
    public static class FileStorageHelper
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] DocumentExtensions = { ".pdf", ".docx", ".xlsx", ".doc", ".xls", ".txt" };
        private static readonly string[] ReportExtensions = { ".pdf", ".xlsx", ".csv" };

        public static string GetCategoryPath(FileCategory category) => category switch
        {
            FileCategory.Avatar => "avatars",
            FileCategory.Report => "reports",
            FileCategory.Export => "exports",
            FileCategory.Import => "imports",
            FileCategory.Document => "documents",
            _ => "misc"
        };

        public static bool IsValidFileType(string fileName, FileCategory category)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return category switch
            {
                FileCategory.Avatar => ImageExtensions.Contains(extension),
                FileCategory.Document => DocumentExtensions.Contains(extension),
                FileCategory.Report => ReportExtensions.Contains(extension),
                FileCategory.Export => true, // Allow any file type for exports
                FileCategory.Import => true, // Allow any file type for imports
                _ => false
            };
        }

        public static long GetMaxFileSizeBytes(FileCategory category) => category switch
        {
            FileCategory.Avatar => 2 * 1024 * 1024, // 2MB
            FileCategory.Report => 50 * 1024 * 1024, // 50MB
            FileCategory.Export => 100 * 1024 * 1024, // 100MB
            FileCategory.Import => 100 * 1024 * 1024, // 100MB
            FileCategory.Document => 10 * 1024 * 1024, // 10MB
            _ => 10 * 1024 * 1024 // 10MB default
        };

        public static string GenerateUniqueFileName(string originalFileName, string userId)
        {
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            return $"{userId}_{timestamp}_{guid}{extension}";
        }
    }
}
