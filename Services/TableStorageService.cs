using Azure;
using Azure.Core;
using Azure.Data.Tables;
using KoneMoshapoRetail.Models;
using Microsoft.Extensions.Logging;

namespace KoneMoshapoRetail.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly TableClient _customerTable;
        private readonly TableClient _productTable;
        private readonly ILogger<TableStorageService> _logger;
        private const string CustomerPartitionKey = "Customer";
        private const string ProductPartitionKey = "Product";

        public TableStorageService(IConfiguration configuration, ILogger<TableStorageService> logger)
        {
            _logger = logger;
            try
            {
                var connectionString = configuration["AzureStorage:ConnectionString"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("❌ Azure Storage connection string is not configured.");
                }

                // Configure retry policy for resilience 🔄
                var tableOptions = new TableClientOptions
                {
                    Retry = {
                        Delay = TimeSpan.FromSeconds(2),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential,
                        MaxDelay = TimeSpan.FromSeconds(60)
                    }
                };

                var tableServiceClient = new TableServiceClient(connectionString, tableOptions);

                _customerTable = tableServiceClient.GetTableClient("CustomerProfiles");
                _productTable = tableServiceClient.GetTableClient("ProductInfo");

                // Create tables if they don't exist ✅
                _customerTable.CreateIfNotExists();
                _productTable.CreateIfNotExists();

                _logger.LogInformation("✅ Table Storage initialized successfully for KoneMoshapoRetail");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to initialize Table Storage: {ex.Message}");
                throw;
            }
        }

        // 👤 CUSTOMER METHODS
        public async Task<bool> AddCustomerAsync(CustomerProfile customer)
        {
            try
            {
                if (customer == null)
                    throw new ArgumentNullException(nameof(customer));

                customer.PartitionKey = CustomerPartitionKey;
                customer.RowKey = Guid.NewGuid().ToString();
                customer.CreatedDate = DateTime.UtcNow;

                var response = await _customerTable.AddEntityAsync(customer);
                _logger.LogInformation($"✅ Customer '{customer.CustomerName}' added successfully with ID: {customer.RowKey}");
                return response.Status == 204;
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                _logger.LogError($"❌ Conflict adding customer: {ex.Message}");
                throw new ApplicationException("A customer with this ID already exists.");
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError($"❌ Table not found: {ex.Message}");
                throw new ApplicationException("The customer table could not be found.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Unexpected error adding customer: {ex.Message}");
                throw new ApplicationException($"An error occurred while adding the customer: {ex.Message}");
            }
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            try
            {
                var customers = new List<CustomerProfile>();
                var query = _customerTable.QueryAsync<CustomerProfile>(
                    filter: $"PartitionKey eq '{CustomerPartitionKey}'"
                );

                await foreach (var customer in query)
                {
                    customers.Add(customer);
                }

                _logger.LogInformation($"📊 Retrieved {customers.Count} customers from Table Storage");
                return customers;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"❌ Failed to retrieve customers: {ex.Message}");
                throw new ApplicationException("An error occurred while retrieving customers.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Unexpected error retrieving customers: {ex.Message}");
                throw;
            }
        }

        public async Task<CustomerProfile> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                    throw new ArgumentException("Partition key and row key cannot be null or empty.");

                var response = await _customerTable.GetEntityAsync<CustomerProfile>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"⚠️ Customer not found: PartitionKey={partitionKey}, RowKey={rowKey}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving customer: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateCustomerAsync(CustomerProfile customer)
        {
            try
            {
                if (customer == null)
                    throw new ArgumentNullException(nameof(customer));

                customer.PartitionKey = CustomerPartitionKey;
                var response = await _customerTable.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation($"✏️ Customer '{customer.CustomerName}' updated successfully");
                return response.Status == 204;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError($"❌ Customer not found for update: {ex.Message}");
                throw new KeyNotFoundException("Customer not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating customer: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _customerTable.DeleteEntityAsync(partitionKey, rowKey);
                _logger.LogInformation($"🗑️ Customer with RowKey {rowKey} deleted successfully");
                return response.Status == 204;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"⚠️ Customer not found for deletion: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting customer: {ex.Message}");
                throw;
            }
        }

        // 📦 PRODUCT METHODS
        public async Task<bool> AddProductAsync(ProductInfo product)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                product.PartitionKey = ProductPartitionKey;
                product.RowKey = Guid.NewGuid().ToString();
                product.CreatedAt = DateTime.UtcNow;

                var response = await _productTable.AddEntityAsync(product);
                _logger.LogInformation($"✅ Product '{product.ProductName}' added successfully");
                return response.Status == 204;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error adding product: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ProductInfo>> GetAllProductsAsync()
        {
            try
            {
                var products = new List<ProductInfo>();
                var query = _productTable.QueryAsync<ProductInfo>(
                    filter: $"PartitionKey eq '{ProductPartitionKey}'"
                );

                await foreach (var product in query)
                {
                    products.Add(product);
                }

                _logger.LogInformation($"📊 Retrieved {products.Count} products");
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving products: {ex.Message}");
                throw;
            }
        }

        public async Task<ProductInfo> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productTable.GetEntityAsync<ProductInfo>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"⚠️ Product not found: {rowKey}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving product: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateProductAsync(ProductInfo product)
        {
            try
            {
                product.PartitionKey = ProductPartitionKey;
                var response = await _productTable.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation($"✏️ Product '{product.ProductName}' updated successfully");
                return response.Status == 204;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating product: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productTable.DeleteEntityAsync(partitionKey, rowKey);
                _logger.LogInformation($"🗑️ Product deleted: {rowKey}");
                return response.Status == 204;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"⚠️ Product not found: {rowKey}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting product: {ex.Message}");
                throw;
            }
        }
    }
}