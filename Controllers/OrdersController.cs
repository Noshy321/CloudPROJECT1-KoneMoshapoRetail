using Microsoft.AspNetCore.Mvc;
using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;
using System.Text.Json;

namespace KoneMoshapoRetail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IQueueStorageService _queueService;
        private readonly ITableStorageService _tableService;
        private readonly ILogger<OrdersController> _logger;
        private readonly IFileStorageService _fileService;

        public OrdersController(IQueueStorageService queueService,
                               ITableStorageService tableService,
                               ILogger<OrdersController> logger,
                               IFileStorageService fileService)
        {
            _queueService = queueService;
            _tableService = tableService;
            _logger = logger;
            _fileService = fileService;
        }

        // GET: Orders 📋
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get order messages from queue
                var orderMessages = await _queueService.PeekMessagesAsync("orders", 10);
                var inventoryMessages = await _queueService.PeekMessagesAsync("inventory", 10);

                ViewBag.OrderCount = await _queueService.GetQueueLengthAsync("orders");
                ViewBag.InventoryCount = await _queueService.GetQueueLengthAsync("inventory");

                var orderDetails = new List<OrderMessage>();
                foreach (var msg in orderMessages)
                {
                    try
                    {
                        var decodedMsg = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                        var order = JsonSerializer.Deserialize<OrderMessage>(decodedMsg);
                        if (order != null)
                            orderDetails.Add(order);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"⚠️ Could not deserialize order message: {ex.Message}");
                    }
                }

                ViewBag.OrderMessages = orderDetails;
                ViewBag.InventoryMessages = inventoryMessages;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading orders: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load orders. Please try again.";
                return View();
            }
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var customers = await _tableService.GetAllCustomersAsync();
                var products = await _tableService.GetAllProductsAsync();

                ViewBag.Customers = customers.Where(c => c.IsActive).ToList();
                ViewBag.Products = products.Where(p => p.IsAvailable && p.StockQuantity > 0).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading order creation form: {ex.Message}");
                TempData["ErrorMessage"] = "❌ Failed to load order creation form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderMessage order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate customer exists
                    var customer = await _tableService.GetCustomerAsync("Customer", order.CustomerId);
                    if (customer == null)
                    {
                        TempData["ErrorMessage"] = "⚠️ Customer not found.";
                        return RedirectToAction(nameof(Create));
                    }

                    // Validate products exist and have enough stock
                    foreach (var item in order.Items)
                    {
                        var product = await _tableService.GetProductAsync("Product", item.ProductId);
                        if (product == null)
                        {
                            TempData["ErrorMessage"] = $"⚠️ Product '{item.ProductName}' not found.";
                            return RedirectToAction(nameof(Create));
                        }

                        if (product.StockQuantity < item.Quantity)
                        {
                            TempData["ErrorMessage"] = $"⚠️ Insufficient stock for '{product.ProductName}'. Available: {product.StockQuantity}";
                            return RedirectToAction(nameof(Create));
                        }
                    }

                    // Set order details
                    order.OrderId = $"KMR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                    order.OrderDate = DateTime.UtcNow;
                    order.Status = "Pending";
                    order.CustomerName = customer.CustomerName;
                    order.TotalAmount = order.Items.Sum(i => i.Subtotal);

                    // Send to queue
                    var result = await _queueService.SendOrderMessageAsync(order);

                    if (result)
                    {
                        TempData["SuccessMessage"] = $"✅ Order {order.OrderId} created and queued successfully! 🎉";
                        _logger.LogInformation($"✅ Order created: {order.OrderId} - Total: {order.TotalAmount:C}");

                        // Log activity 📝
                        var logEntry = new LogEntry
                        {
                            Message = $"New order created: {order.OrderId} - Customer: {order.CustomerName} - Total: {order.TotalAmount:C}",
                            Level = "Information",
                            Source = "OrdersController",
                            Action = "Create",
                            UserId = order.CustomerId
                        };
                        await _fileService.UploadLogFileAsync(
                            $"order-created-{DateTime.Now:yyyyMMddHHmmss}.txt",
                            logEntry.ToFormattedString()
                        );

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "❌ Failed to queue order. Please try again.";
                    }
                }

                // Repopulate dropdowns
                var customers = await _tableService.GetAllCustomersAsync();
                var products = await _tableService.GetAllProductsAsync();
                ViewBag.Customers = customers.Where(c => c.IsActive).ToList();
                ViewBag.Products = products.Where(p => p.IsAvailable && p.StockQuantity > 0).ToList();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating order: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to create order: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

        // POST: Orders/Process
        [HttpPost]
        public async Task<IActionResult> Process(string orderId)
        {
            try
            {
                // In a real implementation, you would process the order here
                // Update status, adjust inventory, etc.

                _logger.LogInformation($"🔄 Processing order: {orderId}");

                // Log activity 📝
                var logEntry = new LogEntry
                {
                    Message = $"Order processed: {orderId}",
                    Level = "Information",
                    Source = "OrdersController",
                    Action = "Process"
                };
                await _fileService.UploadLogFileAsync(
                    $"order-processed-{DateTime.Now:yyyyMMddHHmmss}.txt",
                    logEntry.ToFormattedString()
                );

                TempData["SuccessMessage"] = $"✅ Order {orderId} processed successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing order: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to process order: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Orders/Queue/Action
        [HttpPost]
        public async Task<IActionResult> QueueAction(string queueName, string messageId, string popReceipt, string action)
        {
            try
            {
                if (action == "delete")
                {
                    var result = await _queueService.DeleteMessageAsync(queueName, messageId, popReceipt);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "🗑️ Message deleted successfully!";
                        _logger.LogInformation($"🗑️ Message deleted from {queueName} queue");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "⚠️ Failed to delete message.";
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error performing queue action: {ex.Message}");
                TempData["ErrorMessage"] = $"❌ Failed to perform action: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}