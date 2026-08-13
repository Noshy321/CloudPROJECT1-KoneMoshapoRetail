using Azure;
using Azure.Core;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using KoneMoshapoRetail.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace KoneMoshapoRetail.Services
{
    public class QueueStorageService : IQueueStorageService
    {
        private readonly QueueClient _orderQueue;
        private readonly QueueClient _inventoryQueue;
        private readonly ILogger<QueueStorageService> _logger;

        public QueueStorageService(
            IConfiguration configuration,
            ILogger<QueueStorageService> logger)
        {
            _logger = logger;

            try
            {
                var connectionString =
                    configuration["AzureStorage:ConnectionString"];

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "❌ Azure Storage connection string is not configured.");
                }

                // Configure Azure Queue Storage retry policy
                var queueOptions = new QueueClientOptions
                {
                    Retry =
                    {
                        Delay = TimeSpan.FromSeconds(2),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential,
                        MaxDelay = TimeSpan.FromSeconds(60)
                    }
                };

                var queueServiceClient =
                    new QueueServiceClient(connectionString, queueOptions);

                // Get queue clients
                _orderQueue =
                    queueServiceClient.GetQueueClient("kone-orders");

                _inventoryQueue =
                    queueServiceClient.GetQueueClient("kone-inventory");

                // Create queues if they do not already exist
                _orderQueue.CreateIfNotExists();
                _inventoryQueue.CreateIfNotExists();

                _logger.LogInformation(
                    "✅ Queue Storage initialized successfully for KoneMoshapoRetail");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "❌ Failed to initialize Queue Storage");

                throw;
            }
        }

        // ============================================================
        // SEND ORDER MESSAGE
        // ============================================================

        public async Task<bool> SendOrderMessageAsync(OrderMessage order)
        {
            try
            {
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order));
                }

                var messageJson = JsonSerializer.Serialize(order);
                var messageBytes = Encoding.UTF8.GetBytes(messageJson);
                var base64Message = Convert.ToBase64String(messageBytes);

                var response = await _orderQueue.SendMessageAsync(base64Message);

                _logger.LogInformation(
                    "📨 Order message sent successfully. OrderId: {OrderId}, Status: {Status}",
                    order.OrderId,
                    order.Status);

                return response.GetRawResponse().Status == 201;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError(ex, "❌ Order queue not found");
                throw new ApplicationException("The order queue could not be found.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending order message");
                throw new ApplicationException($"Failed to send order: {ex.Message}", ex);
            }
        }

        // ============================================================
        // SEND INVENTORY MESSAGE
        // ============================================================

        public async Task<bool> SendInventoryMessageAsync(
            string productId,
            int quantity,
            string action)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
                }

                var inventoryMessage = new
                {
                    ProductId = productId,
                    Quantity = quantity,
                    Action = action,
                    Timestamp = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Source = "KoneMoshapoRetail"
                };

                var messageJson = JsonSerializer.Serialize(inventoryMessage);
                var messageBytes = Encoding.UTF8.GetBytes(messageJson);
                var base64Message = Convert.ToBase64String(messageBytes);

                var response = await _inventoryQueue.SendMessageAsync(base64Message);

                _logger.LogInformation(
                    "📦 Inventory message sent. Product: {ProductId}, Action: {Action}, Qty: {Quantity}",
                    productId,
                    action,
                    quantity);

                return response.GetRawResponse().Status == 201;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending inventory message");
                throw;
            }
        }

        // ============================================================
        // PEEK MESSAGES
        // ============================================================

        public async Task<List<PeekedMessage>> PeekMessagesAsync(
            string queueName,
            int maxMessages = 5)
        {
            try
            {
                var queueClient = GetQueueClient(queueName);

                var response = await queueClient.PeekMessagesAsync(maxMessages);
                var messageList = response.Value.ToList();

                _logger.LogInformation(
                    "👁️ Peeked {Count} messages from {QueueName} queue",
                    messageList.Count,
                    queueName);

                return messageList;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning("⚠️ Queue not found: {QueueName}", queueName);
                return new List<PeekedMessage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error peeking messages from {QueueName} queue", queueName);
                throw;
            }
        }

        // ============================================================
        // DELETE MESSAGE
        // ============================================================

        public async Task<bool> DeleteMessageAsync(
            string queueName,
            string messageId,
            string popReceipt)
        {
            try
            {
                var queueClient = GetQueueClient(queueName);

                var response = await queueClient.DeleteMessageAsync(messageId, popReceipt);

                _logger.LogInformation(
                    "🗑️ Message {MessageId} deleted from {QueueName} queue",
                    messageId,
                    queueName);

                return response.Status == 204;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting message {MessageId} from {QueueName} queue", messageId, queueName);
                throw;
            }
        }

        // ============================================================
        // GET QUEUE LENGTH
        // ============================================================

        public async Task<int> GetQueueLengthAsync(string queueName)
        {
            try
            {
                var queueClient = GetQueueClient(queueName);
                var properties = await queueClient.GetPropertiesAsync();

                _logger.LogInformation(
                    "📊 Queue {QueueName} contains approximately {Count} messages",
                    queueName,
                    properties.Value.ApproximateMessagesCount);

                return properties.Value.ApproximateMessagesCount;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning("⚠️ Queue not found: {QueueName}", queueName);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting queue length for {QueueName}", queueName);
                throw;
            }
        }

        // ============================================================
        // GET QUEUE CLIENT
        // ============================================================

        private QueueClient GetQueueClient(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("Queue name cannot be empty.", nameof(queueName));
            }

            return queueName.Trim().ToLowerInvariant() switch
            {
                "orders" or "kone-orders" => _orderQueue,
                "inventory" or "kone-inventory" => _inventoryQueue,
                _ => throw new ArgumentException($"Invalid queue name: {queueName}")
            };
        }
    }
}