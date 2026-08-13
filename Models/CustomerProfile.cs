using Azure;
using Azure.Data.Tables;

namespace KoneMoshapoRetail.Models
{
    public class CustomerProfile : ITableEntity
    {
        // Primary Keys
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        // Personal Information
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Address Information
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "South Africa";

        // Customer Details
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public string CustomerType { get; set; } = "Standard";
        public bool IsActive { get; set; } = true;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;

        // ITableEntity required properties
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}