namespace ECommerce.Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }

        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
    }
}
