namespace ECommerce.Application.DTOs
{
    public class CartItemUpdateResult
    {
        public bool Success { get; set; }

        public int Quantity { get; set; }
        public string? Message { get; set; }
        public decimal ItemTotal { get; set; }

        public int Count { get; set; }

        public decimal Total { get; set; }
    }
}
