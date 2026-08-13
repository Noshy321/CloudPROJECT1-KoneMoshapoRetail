using Azure.Storage.Files.Shares.Models;
using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace KoneMoshapoRetail.Controllers
{
    public class LogsController : Controller
    {
        private readonly IFileStorageService _fileService;
        private readonly ILogger<LogsController> _logger;

        public LogsController(IFileStorageService fileService, ILogger<LogsController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        // GET: Logs 📝
        public async Task<IActionResult> Index()
        {
            try
            {
                var logs = await _fileService.GetLogFilesAsync();
                ViewBag.LogCount = logs?.Count ?? 0;
                return View(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading logs: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load logs. Please try again.";
                return View(new List<ShareFileItem>());
            }
        }

        // GET: Logs/Details/{fileName}
        public async Task<IActionResult> Details(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    TempData["ErrorMessage"] = "⚠️ No file name provided.";
                    return RedirectToAction(nameof(Index));
                }

                var content = await _fileService.DownloadLogFileAsync(fileName);
                if (content == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Log file not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.FileName = fileName;
                ViewBag.Content = content;

                _logger.LogInformation($"📖 Log file viewed: {fileName}");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading log details: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to load log details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Logs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Logs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LogEntry logEntry)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    logEntry.Timestamp = DateTime.UtcNow;
                    logEntry.LogId = Guid.NewGuid().ToString();

                    var fileName = $"manual-log-{DateTime.Now:yyyyMMddHHmmss}-{logEntry.LogId}.txt";
                    var content = logEntry.ToFormattedString();

                    var result = await _fileService.UploadLogFileAsync(fileName, content);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "✅ Log entry created successfully! 🎉";
                        _logger.LogInformation($"✅ Manual log entry created: {fileName}");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "❌ Failed to create log entry.";
                    }
                }
                return View(logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating log entry: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to create log entry: {ex.Message}";
                return View(logEntry);
            }
        }

        // GET: Logs/Delete/{fileName}
        [HttpGet]
        public async Task<IActionResult> Delete(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    TempData["ErrorMessage"] = "⚠️ No file name provided.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if file exists
                var exists = await _fileService.FileExistsAsync(fileName);
                if (!exists)
                {
                    TempData["ErrorMessage"] = "⚠️ Log file not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get file content to show preview before deletion
                var content = await _fileService.DownloadLogFileAsync(fileName);
                var properties = await _fileService.GetFilePropertiesAsync(fileName);

                ViewBag.FileName = fileName;
                ViewBag.Content = content?.Length > 500 ? content.Substring(0, 500) + "..." : content;
                ViewBag.FileSize = properties?.ContentLength ?? 0;
                // ✅ NEW (works with non-nullable DateTimeOffset)
                ViewBag.LastModified = properties.LastModified.ToString("yyyy-MM-dd HH:mm:ss");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading log for deletion: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to load log: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Logs/Delete/{fileName}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    TempData["ErrorMessage"] = "⚠️ No file name provided.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation($"🗑️ Attempting to delete log file: {fileName}");

                var result = await _fileService.DeleteLogFileAsync(fileName);

                if (result)
                {
                    TempData["SuccessMessage"] = $"🗑️ Log file '{fileName}' deleted successfully!";
                    _logger.LogInformation($"🗑️ Log file deleted successfully: {fileName}");
                }
                else
                {
                    TempData["ErrorMessage"] = "⚠️ Log file not found or could not be deleted.";
                    _logger.LogWarning($"⚠️ Log file could not be deleted: {fileName}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"❌ Access denied: {ex.Message}");
                TempData["ErrorMessage"] = "❌ You don't have permission to delete this file.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting log file: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to delete log file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}