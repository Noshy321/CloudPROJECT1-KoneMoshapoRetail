using System.Net;
using System.Text.Json;
using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace KoneMoshapoRetail.Functions
{
    public class StoreCustomerFunction
    {
        private readonly ITableStorageService _tableService;
        private readonly ILogger _logger;

        public StoreCustomerFunction(ITableStorageService tableService, ILoggerFactory loggerFactory)
        {
            _tableService = tableService;
            _logger = loggerFactory.CreateLogger<StoreCustomerFunction>();
        }

        [Function("StoreCustomer")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Storing customer in Azure Table Storage...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonSerializer.Deserialize<CustomerProfile>(requestBody);

            if (customer == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid customer data.");
                return badResponse;
            }

            var result = await _tableService.AddCustomerAsync(customer);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(result
                ? $"Customer {customer.CustomerName} stored successfully."
                : "Failed to store customer.");
            return response;
        }
    }
}