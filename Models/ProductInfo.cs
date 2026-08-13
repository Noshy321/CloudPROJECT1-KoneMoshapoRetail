using Azure;
using Azure.Data.Tables;

namespace KoneMoshapoRetail.Models
{
    public class ProductInfo : ITableEntity
    {
        // Primary Keys
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        // Product Details
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; } = 0;

        // Inventory
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; } = 10;

        // Media
        public string ImageUrl { get; set; } = string.Empty;

        // Classification
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        // Status
        public bool IsAvailable { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ITableEntity required properties
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}