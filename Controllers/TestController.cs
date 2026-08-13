using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KoneMoshapoRetail.Controllers
{
    public class TestController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TestController> _logger;

        public TestController(IConfiguration configuration, ILogger<TestController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var results = new List<string>();
            results.Add("🔍 Testing Azure Storage Connections...");
            results.Add("");

            var connectionString = _configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                results.Add("❌ Connection string is empty or null!");
                return Content(string.Join("\n", results), "text/plain");
            }

            results.Add("✅ Connection string found!");
            results.Add("");

            // Test 1: Queue Storage
            try
            {
                results.Add("📋 Testing Queue Storage...");
                var queueServiceClient = new QueueServiceClient(connectionString);
                var queueClient = queueServiceClient.GetQueueClient("kone-orders");
                await queueClient.CreateIfNotExistsAsync();
                results.Add("✅ Queue Storage working!");
            }
            catch (Exception ex)
            {
                results.Add($"❌ Queue Storage error: {ex.Message}");
            }

            // Test 2: Blob Storage
            try
            {
                results.Add("🖼️ Testing Blob Storage...");
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("media");
                await containerClient.CreateIfNotExistsAsync();
                results.Add("✅ Blob Storage working!");
            }
            catch (Exception ex)
            {
                results.Add($"❌ Blob Storage error: {ex.Message}");
            }

            // Test 3: Table Storage
            try
            {
                results.Add("📊 Testing Table Storage...");
                var tableServiceClient = new TableServiceClient(connectionString);
                var tableClient = tableServiceClient.GetTableClient("CustomerProfiles");
                await tableClient.CreateIfNotExistsAsync();
                results.Add("✅ Table Storage working!");
            }
            catch (Exception ex)
            {
                results.Add($"❌ Table Storage error: {ex.Message}");
            }

            // Test 4: File Storage
            try
            {
                results.Add("📂 Testing File Storage...");
                var shareServiceClient = new ShareServiceClient(connectionString);
                var shareClient = shareServiceClient.GetShareClient("logs");
                await shareClient.CreateIfNotExistsAsync();
                results.Add("✅ File Storage working!");
            }
            catch (Exception ex)
            {
                results.Add($"❌ File Storage error: {ex.Message}");
            }

            results.Add("");
            results.Add("🎉 All tests completed!");
            results.Add("📌 Navigate to /Home to see the application.");

            return Content(string.Join("\n", results), "text/plain");
        }
    }
}