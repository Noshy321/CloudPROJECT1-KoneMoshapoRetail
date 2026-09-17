using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;

namespace KoneMoshapoRetail.Functions
{
    public class OrderQueueFunction
    {
        private readonly IQueueStorageService _queueService;
        private readonly ILogger<OrderQueueFunction> _logger;

        public OrderQueueFunction(IQueueStorageService queueService, ILogger<OrderQueueFunction> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        [FunctionName("ProcessOrderQueue")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Processing order queue...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<OrderMessage>(requestBody);

            if (order == null)
            {
                return new BadRequestObjectResult("Please pass a valid order object.");
            }

            await _queueService.SendOrderMessageAsync(order);
            var queueLength = await _queueService.GetQueueLengthAsync("kone-orders");

            return new OkObjectResult($"Order {order.OrderId} queued. Current queue length: {queueLength}");
        }
    }
}