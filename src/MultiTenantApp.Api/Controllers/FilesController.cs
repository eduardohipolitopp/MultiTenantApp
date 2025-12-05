using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public FilesController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            _contentTypeProvider = new FileExtensionContentTypeProvider();
        }

        [HttpGet("{*filePath}")]
        [AllowAnonymous] // Allow accessing avatars without auth if needed, or keep it secured
        public async Task<IActionResult> GetFile(string filePath)
        {
            try
            {
                // Decode the file path if it contains special characters
                filePath = System.Net.WebUtility.UrlDecode(filePath);

                var fileStream = await _fileStorageService.GetFileAsync(filePath);
                
                if (!_contentTypeProvider.TryGetContentType(filePath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                return File(fileStream, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
