namespace MultiTenantApp.Web.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Fetches a file from the API and converts it to a base64 data URL
        /// </summary>
        /// <param name="filePath">The file path to fetch (e.g., "avatars/user123.jpg")</param>
        /// <returns>Base64 data URL (e.g., "data:image/jpeg;base64,{base64}") or null if file not found</returns>
        Task<string?> GetFileAsBase64Async(string filePath);
    }
}
