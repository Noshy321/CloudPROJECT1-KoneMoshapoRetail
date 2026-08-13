using Azure.Storage.Blobs.Models;

namespace KoneMoshapoRetail.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<BlobItem>> GetAllBlobsAsync();
        Task<Stream> DownloadBlobAsync(string blobName);
        Task<bool> DeleteBlobAsync(string blobName);
        Task<string> GetBlobUrl(string blobName);
        Task<bool> BlobExistsAsync(string blobName);
    }
}