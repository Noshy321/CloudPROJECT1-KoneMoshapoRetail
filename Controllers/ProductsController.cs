using Microsoft.AspNetCore.Mvc;
using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;

namespace KoneMoshapoRetail.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ITableStorageService _tableService;
        private readonly IBlobStorageService _blobService;
        private readonly ILogger<ProductsController> _logger;
        private readonly IFileStorageService _fileService;

        public ProductsController(ITableStorageService tableService,
                                  IBlobStorageService blobService,
                                  ILogger<ProductsController> logger,
                                  IFileStorageService fileService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _logger = logger;
            _fileService = fileService;
        }

        // GET: Products 📦
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _tableService.GetAllProductsAsync();
                ViewBag.ProductCount = products.Count;
                ViewBag.AvailableProducts = products.Count(p => p.IsAvailable);
                ViewBag.AveragePrice = products.Any() ? products.Average(p => p.Price).ToString("C") : "N/A";

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading products: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load products. Please try again.";
                return View(new List<ProductInfo>());
            }
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInfo product, IFormFile imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Upload image if provided 🖼️
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _blobService.UploadImageAsync(imageFile);
                        product.ImageUrl = imageUrl;
                        _logger.LogInformation($"📤 Image uploaded for product: {product.ProductName}");
                    }

                    var result = await _tableService.AddProductAsync(product);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "✅ Product created successfully! 🎉";
                        _logger.LogInformation($"✅ New product created: {product.ProductName}");

                        // Log activity 📝
                        var logEntry = new LogEntry
                        {
                            Message = $"New product created: {product.ProductName} - Price: {product.Price:C}",
                            Level = "Information",
                            Source = "ProductsController",
                            Action = "Create",
                            UserId = product.RowKey
                        };
                        await _fileService.UploadLogFileAsync(
                            $"product-created-{DateTime.Now:yyyyMMddHHmmss}.txt",
                            logEntry.ToFormattedString()
                        );

                        return RedirectToAction(nameof(Index));
                    }
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating product: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to create product: {ex.Message}";
                return View(product);
            }
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _tableService.GetProductAsync(partitionKey, rowKey);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Product not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading product for edit: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, ProductInfo product, IFormFile imageFile)
        {
            try
            {
                if (partitionKey != product.PartitionKey || rowKey != product.RowKey)
                {
                    TempData["ErrorMessage"] = "⚠️ Product data mismatch.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    // Upload new image if provided 🖼️
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _blobService.UploadImageAsync(imageFile);
                        product.ImageUrl = imageUrl;

                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            var oldBlobName = Path.GetFileName(new Uri(product.ImageUrl).LocalPath);
                            await _blobService.DeleteBlobAsync(oldBlobName);
                        }
                    }

                    var result = await _tableService.UpdateProductAsync(product);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "✏️ Product updated successfully! ✅";
                        _logger.LogInformation($"✏️ Product updated: {product.ProductName}");

                        // Log activity 📝
                        var logEntry = new LogEntry
                        {
                            Message = $"Product updated: {product.ProductName} - Price: {product.Price:C}",
                            Level = "Information",
                            Source = "ProductsController",
                            Action = "Edit",
                            UserId = product.RowKey
                        };
                        await _fileService.UploadLogFileAsync(
                            $"product-updated-{DateTime.Now:yyyyMMddHHmmss}.txt",
                            logEntry.ToFormattedString()
                        );

                        return RedirectToAction(nameof(Index));
                    }
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating product: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to update product: {ex.Message}";
                return View(product);
            }
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _tableService.GetProductAsync(partitionKey, rowKey);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Product not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading product for delete: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _tableService.GetProductAsync(partitionKey, rowKey);
                var result = await _tableService.DeleteProductAsync(partitionKey, rowKey);

                if (result)
                {
                    // Delete associated image from blob storage if exists 🗑️
                    if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var blobName = Path.GetFileName(new Uri(product.ImageUrl).LocalPath);
                        await _blobService.DeleteBlobAsync(blobName);
                        _logger.LogInformation($"🗑️ Product image deleted: {blobName}");
                    }

                    TempData["SuccessMessage"] = $"🗑️ Product '{product?.ProductName}' deleted successfully!";
                    _logger.LogInformation($"🗑️ Product deleted: {product?.ProductName}");

                    // Log activity 📝
                    var logEntry = new LogEntry
                    {
                        Message = $"Product deleted: {product?.ProductName}",
                        Level = "Warning",
                        Source = "ProductsController",
                        Action = "Delete",
                        UserId = rowKey
                    };
                    await _fileService.UploadLogFileAsync(
                        $"product-deleted-{DateTime.Now:yyyyMMddHHmmss}.txt",
                        logEntry.ToFormattedString()
                    );
                }
                else
                {
                    TempData["ErrorMessage"] = "⚠️ Product not found or already deleted.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting product: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to delete product: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _tableService.GetProductAsync(partitionKey, rowKey);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Product not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading product details: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load product details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}