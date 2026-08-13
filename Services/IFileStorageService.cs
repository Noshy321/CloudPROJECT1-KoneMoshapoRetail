using Azure.Storage.Files.Shares.Models;

namespace KoneMoshapoRetail.Services
{
    public interface IFileStorageService
    {
        Task<bool> UploadLogFileAsync(string fileName, string content);
        Task<List<ShareFileItem>> GetLogFilesAsync();
        Task<string> DownloadLogFileAsync(string fileName);
        Task<bool> DeleteLogFileAsync(string fileName);
        Task<bool> CreateDirectoryAsync(string directoryName);
        Task<IDictionary<string, string>> GetFileMetadataAsync(string fileName);

        // ✅ New helper methods
        Task<bool> FileExistsAsync(string fileName);
        Task<ShareFileProperties> GetFilePropertiesAsync(string fileName);
    }
}