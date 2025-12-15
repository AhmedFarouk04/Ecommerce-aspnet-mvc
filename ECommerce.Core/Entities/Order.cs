using ECommerce.Core.Enums;
using System;
using System.Collections.Generic;

using ECommerce.Core.Enums;
namespace ECommerce.Core.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string FullName { get; set; }
        public string Address { get; set; }

      
        public string? PaymentReference { get; set; }

        public decimal TotalAmount { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>(); public PaymentStatus PaymentStatus { get; set; }
        public OrderStatus Status { get; set; }
    }
}
