namespace ECommerce.Application.DTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public string ProductImage { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Total => Quantity * UnitPrice;
    }
}
