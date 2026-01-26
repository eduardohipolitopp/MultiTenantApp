using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using global::MultiTenantApp.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiTenantApp.Domain.Enums;
using MultiTenantApp.Domain.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantApp.Infrastructure.Services
{
   

    namespace MultiTenantApp.Infrastructure.Services
    {
        /// <summary>
        /// S3-compatible file storage service. Works with AWS S3 or MinIO (S3-compatible).
        /// Configuration is read from "FileStorage:S3" section (see example below).
        /// </summary>
        public class S3FileStorageService : IFileStorageService, IDisposable
        {
            private readonly AmazonS3Client _s3Client;
            private readonly string _bucket;
            private readonly ILogger<S3FileStorageService> _logger;
            private readonly bool _usePresignedUrls;
            private readonly TimeSpan _presignedUrlTtl; 
            private readonly bool _forcePathStyle;
            private readonly Uri? _serviceUri;


            public S3FileStorageService(IConfiguration configuration, ILogger<S3FileStorageService> logger)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));

                var cfg = configuration.GetSection("FileStorage:S3");
                if (!cfg.Exists())
                    throw new ArgumentException("Missing configuration section 'FileStorage:S3'");

                var accessKey = cfg["AccessKey"];
                var secretKey = cfg["SecretKey"];
                _bucket = cfg["Bucket"] ?? throw new ArgumentException("FileStorage:S3:Bucket is required");
                var serviceUrl = cfg["ServiceURL"]; // optional; if set, uses that endpoint (MinIO)
                var region = cfg["Region"] ?? "us-east-1";
                var forcePathStyle = bool.TryParse(cfg["ForcePathStyle"], out var fps) && fps;
                _usePresignedUrls = bool.TryParse(cfg["UsePresignedUrls"], out var up) && up;
                _presignedUrlTtl = TimeSpan.FromSeconds(int.TryParse(cfg["PresignedUrlTtlSeconds"], out var ttl) ? ttl : 3600);

                // Build S3 config
                var s3Config = new AmazonS3Config
                {
                    ForcePathStyle = forcePathStyle
                };
                _forcePathStyle = forcePathStyle;

                if (!string.IsNullOrWhiteSpace(serviceUrl))
                {
                    s3Config.ServiceURL = serviceUrl.TrimEnd('/');
                    _serviceUri = new Uri(serviceUrl.TrimEnd('/'));

                    // RegionEndpoint can be left default when using ServiceURL
                }
                else
                {
                    // If no service URL provided, use region endpoint
                    s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(region);
                }

                // Create client (will use provided credentials)
                if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
                {
                    // If no keys provided, rely on environment/instance role / default chain
                    _s3Client = new AmazonS3Client(s3Config);
                }
                else
                {
                    _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
                }
            }

            #region IFileStorageService (public)

            public async Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string userId) =>
                await SaveInternalAsync(fileStream, fileName, FileCategory.Avatar, userId);

            public async Task DeleteAvatarAsync(string avatarUrl)
            {
                if (string.IsNullOrEmpty(avatarUrl))
                    return;

                // avatarUrl is expected to be stored path like "avatars/xyz.jpg" or full url
                var key = ExtractKeyFromUrlOrPath(avatarUrl);
                await DeleteFileAsync(key);
            }

            public async Task<string> SaveReportAsync(Stream fileStream, string fileName, string userId) =>
                await SaveInternalAsync(fileStream, fileName, FileCategory.Report, userId);

            public async Task<string> SaveExportAsync(Stream fileStream, string fileName, string userId) =>
                await SaveInternalAsync(fileStream, fileName, FileCategory.Export, userId);

            public async Task<string> SaveImportAsync(Stream fileStream, string fileName, string userId) =>
                await SaveInternalAsync(fileStream, fileName, FileCategory.Import, userId);

            public async Task<string> SaveDocumentAsync(Stream fileStream, string fileName, string userId) =>
                await SaveInternalAsync(fileStream, fileName, FileCategory.Document, userId);

            public async Task<string> SaveFileAsync(Stream fileStream, string fileName, FileCategory category, string userId) =>
                await SaveInternalAsync(fileStream, fileName, category, userId);

            public async Task<Stream> GetFileAsync(string filePath)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("filePath is required");

                var key = NormalizeKey(filePath);

                try
                {
                    var resp = await _s3Client.GetObjectAsync(new GetObjectRequest
                    {
                        BucketName = _bucket,
                        Key = key
                    });

                    // copy to MemoryStream because response stream will be disposed with response
                    var ms = new MemoryStream();
                    await resp.ResponseStream.CopyToAsync(ms);
                    ms.Position = 0;
                    resp.Dispose();
                    return ms;
                }
                catch (AmazonS3Exception aex) when (aex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(aex, "S3 object not found: {Bucket}/{Key}", _bucket, key);
                    throw new FileNotFoundException($"File not found: {filePath}", aex);
                }
            }

            public async Task DeleteFileAsync(string filePath)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                var key = NormalizeKey(filePath);

                try
                {
                    await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = _bucket,
                        Key = key
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting object {Bucket}/{Key}", _bucket, key);
                    throw;
                }
            }

            public string GetFileUrl(string fileName, FileCategory category)
            {
                var categoryPath = GetCategoryPath(category);
                var key = $"{categoryPath.TrimEnd('/')}/{fileName}".TrimStart('/');

                if (_usePresignedUrls)
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = _bucket,
                        Key = key,
                        Expires = DateTime.UtcNow.Add(_presignedUrlTtl),
                        Verb = HttpVerb.GET
                    };
                    return _s3Client.GetPreSignedURL(request);
                }

                if (_serviceUri != null)
                {
                    var authority = _serviceUri.GetLeftPart(UriPartial.Authority).TrimEnd('/'); // e.g. http://minio:9000

                    if (_forcePathStyle)
                    {
                        // path-style: {service}/{bucket}/{key}
                        return $"{authority}/{_bucket}/{Uri.EscapeDataString(key)}";
                    }
                    else
                    {
                        // virtual-hosted style: {scheme}://{bucket}.{host[:port]}/{key}
                        var hostPort = _serviceUri.IsDefaultPort ? _serviceUri.Host : $"{_serviceUri.Host}:{_serviceUri.Port}";
                        var scheme = _serviceUri.Scheme; // "http" or "https"
                        return $"{scheme}://{_bucket}.{hostPort}/{Uri.EscapeDataString(key)}";
                    }
                }

                // Fallback to standard AWS S3 virtual-hosted style
                return $"https://{_bucket}.s3.amazonaws.com/{Uri.EscapeDataString(key)}";
            }


            #endregion

            #region Helpers

            private async Task<string> SaveInternalAsync(Stream fileStream, string fileName, FileCategory category, string userId)
            {
                if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(nameof(fileName));

                // Validate file size / type if you have helpers in your project
                // If FileStorageHelper exists in your solution, you can call it here.
                try
                {
                    // Ensure stream readable and positioned
                    if (!fileStream.CanRead)
                        throw new InvalidOperationException("Stream is not readable");

                    long contentLength = -1;
                    if (fileStream.CanSeek)
                    {
                        contentLength = fileStream.Length - fileStream.Position;
                        fileStream.Position = fileStream.Position; // leave position where it is
                    }
                    else
                    {
                        // If not seekable, copy to MemoryStream to get length (minio/aws can stream without length in some cases but TransferUtility may require it)
                        var ms = new MemoryStream();
                        await fileStream.CopyToAsync(ms);
                        ms.Position = 0;
                        fileStream = ms;
                        contentLength = ms.Length;
                    }

                    var categoryPath = GetCategoryPath(category);
                    var uniqueFileName = GenerateUniqueFileName(fileName, userId);
                    var key = $"{categoryPath.TrimEnd('/')}/{uniqueFileName}".TrimStart('/');

                    // PutObjectRequest with stream
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = key,
                        InputStream = fileStream,
                        AutoCloseStream = false,
                        ContentType = "application/octet-stream"
                    };

                    // If we know the length, set it (helps some servers)
                    if (contentLength >= 0 && contentLength <= int.MaxValue)
                        putRequest.InputStream = fileStream; // already set
                                                             // Use TransferUtility to handle large files better
                    using var transferUtil = new TransferUtility(_s3Client);
                    await transferUtil.UploadAsync(new TransferUtilityUploadRequest
                    {
                        InputStream = fileStream,
                        BucketName = _bucket,
                        Key = key,
                        ContentType = putRequest.ContentType,
                        AutoCloseStream = false
                    });

                    // Return relative path used by application (category/file) so your app stores that
                    return $"{categoryPath.TrimEnd('/')}/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file to S3 bucket {Bucket}", _bucket);
                    throw;
                }
            }

            private static string GenerateUniqueFileName(string fileName, string userId)
            {
                var ext = Path.GetExtension(fileName);
                var name = Path.GetFileNameWithoutExtension(fileName);
                var id = string.IsNullOrWhiteSpace(userId) ? Guid.NewGuid().ToString("N") : $"{userId}-{Guid.NewGuid():N}";
                return $"{name}-{id}{ext}";
            }

            private static string GetCategoryPath(FileCategory category)
            {
                // If you already have FileStorageHelper.GetCategoryPath in your project, call it instead.
                // Here we provide a fallback mapping:
                return category switch
                {
                    FileCategory.Avatar => "avatars",
                    FileCategory.Report => "reports",
                    FileCategory.Export => "exports",
                    FileCategory.Import => "imports",
                    FileCategory.Document => "documents",
                    _ => "files"
                };
            }

            private static string NormalizeKey(string filePath)
            {
                if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Attempt to extract path after bucket if user stored full URL
                    var uri = new Uri(filePath);
                    return uri.AbsolutePath.TrimStart('/');
                }
                return filePath.TrimStart('/');
            }

            private static string ExtractKeyFromUrlOrPath(string urlOrPath)
            {
                if (string.IsNullOrWhiteSpace(urlOrPath)) return urlOrPath!;
                if (urlOrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var u = new Uri(urlOrPath);
                    return u.AbsolutePath.TrimStart('/');
                }
                return urlOrPath.TrimStart('/');
            }

            #endregion

            #region Dispose

            public void Dispose()
            {
                _s3Client?.Dispose();
            }

            #endregion
        }
    }

}
