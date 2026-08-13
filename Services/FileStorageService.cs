using Azure;
using Azure.Core;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace KoneMoshapoRetail.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ShareClient _shareClient;
        private readonly ILogger<FileStorageService> _logger;
        private const string ShareName = "kone-logs";
        private const string DirectoryName = "application-logs";

        public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
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
                var fileOptions = new ShareClientOptions
                {
                    Retry = {
                        Delay = TimeSpan.FromSeconds(2),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential,
                        MaxDelay = TimeSpan.FromSeconds(60)
                    }
                };

                var shareServiceClient = new ShareServiceClient(connectionString, fileOptions);
                _shareClient = shareServiceClient.GetShareClient(ShareName);

                // Create share and directory if they don't exist 📁
                _shareClient.CreateIfNotExists();

                _logger.LogInformation("✅ File Storage initialized successfully for KoneMoshapoRetail");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to initialize File Storage: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UploadLogFileAsync(string fileName, string content)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentException("File name cannot be empty.", nameof(fileName));

                if (string.IsNullOrEmpty(content))
                    throw new ArgumentException("Content cannot be empty.", nameof(content));

                var directory = _shareClient.GetDirectoryClient(DirectoryName);
                await directory.CreateIfNotExistsAsync();

                var fileClient = directory.GetFileClient(fileName);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                // Upload file with metadata 📄
                var metadata = new Dictionary<string, string>
                {
                    { "CreatedBy", "KoneMoshapoRetailApp" },
                    { "CreatedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "FileType", "Log" },
                    { "Environment", "Production" }
                };

                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);

                _logger.LogInformation($"📤 Log file uploaded: {fileName}");
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError($"❌ Share or directory not found: {ex.Message}");
                throw new ApplicationException("The file share or directory could not be found.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error uploading log file: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ShareFileItem>> GetLogFilesAsync()
        {
            try
            {
                var files = new List<ShareFileItem>();
                var directory = _shareClient.GetDirectoryClient(DirectoryName);

                await foreach (var item in directory.GetFilesAndDirectoriesAsync())
                {
                    if (item.IsDirectory != true)
                    {
                        files.Add(item);
                    }
                }

                _logger.LogInformation($"📊 Retrieved {files.Count} log files");
                return files;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"⚠️ Directory not found: {DirectoryName}");
                return new List<ShareFileItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving log files: {ex.Message}");
                throw;
            }
        }

        public async Task<string> DownloadLogFileAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentException("File name cannot be empty.", nameof(fileName));

                var directory = _shareClient.GetDirectoryClient(DirectoryName);
                var fileClient = directory.GetFileClient(fileName);

                // Check if file exists first
                var exists = await fileClient.ExistsAsync();
                if (!exists.Value)
                {
                    _logger.LogWarning($"⚠️ Log file not found: {fileName}");
                    return null;
                }

                var response = await fileClient.DownloadAsync();
                using var reader = new StreamReader(response.Value.Content);
                var content = await reader.ReadToEndAsync();

                _logger.LogInformation($"📥 Downloaded log file: {fileName}");
                return content;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"⚠️ Log file not found: {fileName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error downloading log file: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteLogFileAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    _logger.LogWarning("⚠️ File name is empty");
                    return false;
                }

                _logger.LogInformation($"🗑️ Attempting to delete log file: {fileName}");

                var directory = _shareClient.GetDirectoryClient(DirectoryName);
                var fileClient = directory.GetFileClient(fileName);

                // ✅ Check if file exists before trying to delete
                var exists = await fileClient.ExistsAsync();
                if (!exists.Value)
                {
                    _logger.LogWarning($"⚠️ Log file not found: {fileName}");
                    return false;
                }

                // ✅ Delete the file
                var response = await fileClient.DeleteIfExistsAsync();

                if (response.Value)
                {
                    _logger.LogInformation($"🗑️ Log file deleted successfully: {fileName}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"⚠️ Failed to delete log file: {fileName}");
                    return false;
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"⚠️ Log file not found during deletion: {fileName}");
                return false;
            }
            catch (RequestFailedException ex) when (ex.Status == 403)
            {
                _logger.LogError($"❌ Access denied when deleting file: {fileName}");
                throw new UnauthorizedAccessException($"You don't have permission to delete this file: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting log file: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateDirectoryAsync(string directoryName)
        {
            try
            {
                if (string.IsNullOrEmpty(directoryName))
                {
                    _logger.LogWarning("⚠️ Directory name is empty");
                    return false;
                }

                var directory = _shareClient.GetDirectoryClient(directoryName);
                var response = await directory.CreateIfNotExistsAsync();

                if (response.Value != null)
                {
                    _logger.LogInformation($"📁 Directory created: {directoryName}");
                }
                else
                {
                    _logger.LogInformation($"📁 Directory already exists: {directoryName}");
                }

                return response.Value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating directory: {ex.Message}");
                throw;
            }
        }

        // ✅ Get file metadata as IDictionary
        public async Task<IDictionary<string, string>> GetFileMetadataAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    _logger.LogWarning("⚠️ File name is empty");
                    return new Dictionary<string, string>();
                }

                var directory = _shareClient.GetDirectoryClient(DirectoryName);
                var fileClient = directory.GetFileClient(fileName);

                // Check if file exists
                var exists = await fileClient.ExistsAsync();
                if (!exists.Value)
                {
                    _logger.LogWarning($"⚠️ File not found: {fileName}");
                    return new Dictionary<string, string>();
                }

                var properties = await fileClient.GetPropertiesAsync();

                // Return metadata as Dictionary
                return new Dictionary<string, string>(properties.Value.Metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting file metadata: {ex.Message}");
                throw;
            }
        }

        // ✅ Additional helper method to check if file exists
        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return false;

                var directory = _shareClient.GetDirectoryClient(DirectoryName);
                var fileClient = directory.GetFileClient(fileName);
                return await fileClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking file existence: {ex.Message}");
                return false;
            }
        }

        // ✅ Additional helper method to get file properties
        public async Task<ShareFileProperties> GetFilePropertiesAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentException("File name cannot be empty.", nameof(fileName));

                var directory = _shareClient.GetDirectoryClient(DirectoryName);
                var fileClient = directory.GetFileClient(fileName);

                var properties = await fileClient.GetPropertiesAsync();
                return properties.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting file properties: {ex.Message}");
                throw;
            }
        }
    }
}