using System.Collections.Generic;
using System.Linq;

namespace ECommerce.Application.DTOs
{
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();

        public int Count => Items.Sum(x => x.Quantity);

        public decimal Total => Items.Sum(x => x.Total);

        public bool IsEmpty => !Items.Any();
    }
}
