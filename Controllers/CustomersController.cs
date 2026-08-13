using Microsoft.AspNetCore.Mvc;
using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;

namespace KoneMoshapoRetail.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ITableStorageService _tableService;
        private readonly ILogger<CustomersController> _logger;
        private readonly IFileStorageService _fileService;

        public CustomersController(ITableStorageService tableService,
                                   ILogger<CustomersController> logger,
                                   IFileStorageService fileService)
        {
            _tableService = tableService;
            _logger = logger;
            _fileService = fileService;
        }

        // GET: Customers 👤
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _tableService.GetAllCustomersAsync();
                ViewBag.CustomerCount = customers.Count;
                ViewBag.ActiveCustomers = customers.Count(c => c.IsActive);

                // Log activity 📝
                var logEntry = new LogEntry
                {
                    Message = $"Customer list viewed - {customers.Count} customers found",
                    Level = "Information",
                    Source = "CustomersController",
                    Action = "Index"
                };
                await _fileService.UploadLogFileAsync(
                    $"customer-view-{DateTime.Now:yyyyMMddHHmmss}.txt",
                    logEntry.ToFormattedString()
                );

                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading customers: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load customers. Please try again.";
                return View(new List<CustomerProfile>());
            }
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _tableService.AddCustomerAsync(customer);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "✅ Customer created successfully! 🎉";
                        _logger.LogInformation($"✅ New customer created: {customer.CustomerName}");

                        // Log activity 📝
                        var logEntry = new LogEntry
                        {
                            Message = $"New customer created: {customer.CustomerName} ({customer.Email})",
                            Level = "Information",
                            Source = "CustomersController",
                            Action = "Create",
                            UserId = customer.RowKey
                        };
                        await _fileService.UploadLogFileAsync(
                            $"customer-created-{DateTime.Now:yyyyMMddHHmmss}.txt",
                            logEntry.ToFormattedString()
                        );

                        return RedirectToAction(nameof(Index));
                    }
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating customer: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to create customer: {ex.Message}";
                return View(customer);
            }
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Customer not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading customer for edit: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load customer details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, CustomerProfile customer)
        {
            try
            {
                if (partitionKey != customer.PartitionKey || rowKey != customer.RowKey)
                {
                    TempData["ErrorMessage"] = "⚠️ Customer data mismatch.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    var result = await _tableService.UpdateCustomerAsync(customer);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "✏️ Customer updated successfully! ✅";
                        _logger.LogInformation($"✏️ Customer updated: {customer.CustomerName}");

                        // Log activity 📝
                        var logEntry = new LogEntry
                        {
                            Message = $"Customer updated: {customer.CustomerName} ({customer.Email})",
                            Level = "Information",
                            Source = "CustomersController",
                            Action = "Edit",
                            UserId = customer.RowKey
                        };
                        await _fileService.UploadLogFileAsync(
                            $"customer-updated-{DateTime.Now:yyyyMMddHHmmss}.txt",
                            logEntry.ToFormattedString()
                        );

                        return RedirectToAction(nameof(Index));
                    }
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating customer: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to update customer: {ex.Message}";
                return View(customer);
            }
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Customer not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading customer for delete: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load customer details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
                var result = await _tableService.DeleteCustomerAsync(partitionKey, rowKey);

                if (result)
                {
                    TempData["SuccessMessage"] = $"🗑️ Customer '{customer?.CustomerName}' deleted successfully!";
                    _logger.LogInformation($"🗑️ Customer deleted: {customer?.CustomerName}");

                    // Log activity 📝
                    var logEntry = new LogEntry
                    {
                        Message = $"Customer deleted: {customer?.CustomerName}",
                        Level = "Warning",
                        Source = "CustomersController",
                        Action = "Delete",
                        UserId = rowKey
                    };
                    await _fileService.UploadLogFileAsync(
                        $"customer-deleted-{DateTime.Now:yyyyMMddHHmmss}.txt",
                        logEntry.ToFormattedString()
                    );
                }
                else
                {
                    TempData["ErrorMessage"] = "⚠️ Customer not found or already deleted.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting customer: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to delete customer: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Customer not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading customer details: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load customer details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}