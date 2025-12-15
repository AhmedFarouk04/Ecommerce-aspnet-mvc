using ECommerce.Core.Enums;

namespace ECommerce.Web.ViewModels
{
    public class AdminOrderViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }

        public string? UserImage { get; set; }
        public string? UserThumbnail { get; set; }

        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }

        public int ItemsCount { get; set; }
    }
}
