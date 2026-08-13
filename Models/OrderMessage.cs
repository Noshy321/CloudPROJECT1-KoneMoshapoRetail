namespace KoneMoshapoRetail.Models
{
    public class OrderMessage
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;          // ✅ ADD THIS
        public decimal TaxAmount { get; set; } = 0;               // ✅ ADD THIS
        public decimal FinalAmount { get; set; }                  // ✅ ADD THIS
        public string Status { get; set; } = "Pending";
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;  // ✅ ADD THIS
        public string ShippingProvince { get; set; } = string.Empty; // ✅ ADD THIS
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "Pending";    // ✅ ADD THIS
        public string OrderNotes { get; set; } = string.Empty;
        public bool IsGift { get; set; } = false;                 // ✅ ADD THIS
    }

    public class OrderItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountAmount { get; set; } = 0;          // ✅ ADD THIS
        public decimal Subtotal => (Quantity * Price) - DiscountAmount;
        public bool IsDigital { get; set; } = false;              // ✅ ADD THIS
    }
}