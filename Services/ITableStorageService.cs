using KoneMoshapoRetail.Models;

namespace KoneMoshapoRetail.Services
{
    public interface ITableStorageService
    {
        // Customer methods 👤
        Task<bool> AddCustomerAsync(CustomerProfile customer);
        Task<List<CustomerProfile>> GetAllCustomersAsync();
        Task<CustomerProfile> GetCustomerAsync(string partitionKey, string rowKey);
        Task<bool> UpdateCustomerAsync(CustomerProfile customer);
        Task<bool> DeleteCustomerAsync(string partitionKey, string rowKey);

        // Product methods 📦
        Task<bool> AddProductAsync(ProductInfo product);
        Task<List<ProductInfo>> GetAllProductsAsync();
        Task<ProductInfo> GetProductAsync(string partitionKey, string rowKey);
        Task<bool> UpdateProductAsync(ProductInfo product);
        Task<bool> DeleteProductAsync(string partitionKey, string rowKey);
    }
}