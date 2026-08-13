using Azure.Storage.Queues.Models;
using KoneMoshapoRetail.Models;

namespace KoneMoshapoRetail.Services
{
    public interface IQueueStorageService
    {
        Task<bool> SendOrderMessageAsync(OrderMessage order);

        Task<bool> SendInventoryMessageAsync(
            string productId,
            int quantity,
            string action);

        Task<List<PeekedMessage>> PeekMessagesAsync(
            string queueName,
            int maxMessages = 5);

        Task<bool> DeleteMessageAsync(
            string queueName,
            string messageId,
            string popReceipt);

        Task<int> GetQueueLengthAsync(
            string queueName);
    }
}