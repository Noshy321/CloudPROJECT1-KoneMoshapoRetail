using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace KoneMoshapoRetail.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageService> _logger;
        private const string ContainerName = "kone-moshapo-media";

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _logger = logger;
            try
            {
                var connectionString = configuration["AzureStorage:ConnectionString"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("❌ Azure Storage connection string is not configured.");
                }

                // Configure retry policy 🌀
                var blobOptions = new BlobClientOptions
                {
                    Retry = {
                        Delay = TimeSpan.FromSeconds(2),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential,
                        MaxDelay = TimeSpan.FromSeconds(60)
                    }
                };

                var blobServiceClient = new BlobServiceClient(connectionString, blobOptions);
                _containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

                // Create container if it doesn't exist 📁
                _containerClient.CreateIfNotExists(PublicAccessType.Blob);

                _logger.LogInformation("✅ Blob Storage initialized successfully for KoneMoshapoRetail");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to initialize Blob Storage: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("❌ File cannot be null or empty.", nameof(file));
                }

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var blobClient = _containerClient.GetBlobClient(fileName);

                // Set blob metadata 🔖
                var metadata = new Dictionary<string, string>
                {
                    { "OriginalFileName", file.FileName },
                    { "UploadDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "FileSize", file.Length.ToString() },
                    { "ContentType", file.ContentType },
                    { "UploadedBy", "KoneMoshapoRetailApp" }
                };

                await using var stream = file.OpenReadStream();
                var response = await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });

                await blobClient.SetMetadataAsync(metadata);

                _logger.LogInformation($"📤 Image uploaded: {fileName}, Size: {file.Length} bytes");
                return blobClient.Uri.ToString();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError($"❌ Container not found: {ex.Message}");
                throw new ApplicationException("The storage container could not be found.");
            }
            catch (RequestFailedException ex) when (ex.Status == 403)
            {
                _logger.LogError($"❌ Access denied: {ex.Message}");
                throw new UnauthorizedAccessException("You don't have permission to upload files.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Upload failed: {ex.Message}");
                throw new ApplicationException($"An error occurred while uploading the image: {ex.Message}");
            }
        }

        public async Task<List<BlobItem>> GetAllBlobsAsync()
        {
            try
            {
                var blobs = new List<BlobItem>();
                await foreach (var blob in _containerClient.GetBlobsAsync())
                {
                    blobs.Add(blob);
                }
                _logger.LogInformation($"📊 Retrieved {blobs.Count} blobs from storage");
                return blobs;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError($"❌ Container not found: {ex.Message}");
                throw new ApplicationException("The media container could not be found.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving blobs: {ex.Message}");
                throw;
            }
        }

        public async Task<Stream> DownloadBlobAsync(string blobName)
        {
            try
            {
                if (string.IsNullOrEmpty(blobName))
                    throw new ArgumentException("Blob name cannot be null or empty.");

                var blobClient = _containerClient.GetBlobClient(blobName);
                var response = await blobClient.DownloadStreamingAsync();

                _logger.LogInformation($"📥 Downloaded blob: {blobName}");
                return response.Value.Content;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"⚠️ Blob not found: {blobName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error downloading blob: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteBlobAsync(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                var response = await blobClient.DeleteIfExistsAsync();

                if (response.Value)
                {
                    _logger.LogInformation($"🗑️ Blob deleted: {blobName}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Blob not found: {blobName}");
                }

                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting blob: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetBlobUrl(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                var exists = await blobClient.ExistsAsync();
                return exists.Value ? blobClient.Uri.ToString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting blob URL: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> BlobExistsAsync(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                return await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking blob existence: {ex.Message}");
                throw;
            }
        }
    }
}