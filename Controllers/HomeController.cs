using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KoneMoshapoRetail.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITableStorageService _tableService;
        private readonly IQueueStorageService _queueService;
        private readonly IFileStorageService _fileService;

        public HomeController(
            ILogger<HomeController> logger,
            ITableStorageService tableService,
            IQueueStorageService queueService,
            IFileStorageService fileService)
        {
            _logger = logger;
            _tableService = tableService;
            _queueService = queueService;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get customers count
                var customers = await _tableService.GetAllCustomersAsync();
                ViewBag.CustomerCount = customers?.Count ?? 0;

                // Get products count
                var products = await _tableService.GetAllProductsAsync();
                ViewBag.ProductCount = products?.Count ?? 0;

                // Get orders count from queue
                ViewBag.OrderCount = await _queueService.GetQueueLengthAsync("kone-orders");

                // Get log files count
                var logFiles = await _fileService.GetLogFilesAsync();
                ViewBag.LogCount = logFiles?.Count ?? 0;

                _logger.LogInformation($"📊 Dashboard stats: Customers={ViewBag.CustomerCount}, Products={ViewBag.ProductCount}, Orders={ViewBag.OrderCount}, Logs={ViewBag.LogCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading dashboard: {ex.Message}");
                ViewBag.CustomerCount = 0;
                ViewBag.ProductCount = 0;
                ViewBag.OrderCount = 0;
                ViewBag.LogCount = 0;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}