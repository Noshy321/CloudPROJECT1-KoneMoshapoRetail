using Microsoft.AspNetCore.Mvc;
using KoneMoshapoRetail.Services;
using System.Threading.Tasks;

namespace KoneMoshapoRetail.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ITableStorageService _tableService;
        private readonly IQueueStorageService _queueService;
        private readonly IFileStorageService _fileService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ITableStorageService tableService,
            IQueueStorageService queueService,
            IFileStorageService fileService,
            ILogger<DashboardController> logger)
        {
            _tableService = tableService;
            _queueService = queueService;
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                // Get customers count
                var customers = await _tableService.GetAllCustomersAsync();
                var customerCount = customers?.Count ?? 0;

                // Get products count
                var products = await _tableService.GetAllProductsAsync();
                var productCount = products?.Count ?? 0;

                // Get orders count from queue
                var orderCount = await _queueService.GetQueueLengthAsync("kone-orders");

                // Get log files count
                var logFiles = await _fileService.GetLogFilesAsync();
                var logCount = logFiles?.Count ?? 0;

                var stats = new
                {
                    customers = customerCount,
                    products = productCount,
                    orders = orderCount,
                    logs = logCount
                };

                _logger.LogInformation($"📊 Dashboard stats: Customers={customerCount}, Products={productCount}, Orders={orderCount}, Logs={logCount}");

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting dashboard stats: {ex.Message}");
                return Ok(new { customers = 0, products = 0, orders = 0, logs = 0 });
            }
        }
    }
}