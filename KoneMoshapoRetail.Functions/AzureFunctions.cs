using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KoneMoshapoRetail.Functions
{
    public class StoreCustomerFunction
    {
        private readonly ITableStorageService _tableService;
        private readonly ILogger<StoreCustomerFunction> _logger;

        public StoreCustomerFunction(ITableStorageService tableService, ILogger<StoreCustomerFunction> logger)
        {
            _tableService = tableService;
            _logger = logger;
        }

        [FunctionName("StoreCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Storing customer in Azure Table Storage...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonConvert.DeserializeObject<CustomerProfile>(requestBody);

            if (customer == null)
            {
                return new BadRequestObjectResult("Please pass a valid customer object.");
            }

            var result = await _tableService.AddCustomerAsync(customer);

            return result
                ? (ActionResult)new OkObjectResult($"Customer {customer.CustomerName} stored successfully.")
                : new BadRequestObjectResult("Failed to store customer.");
        }
    }
}