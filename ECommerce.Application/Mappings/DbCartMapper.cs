using ECommerce.Application.DTOs;
using ECommerce.Core.Entities;

namespace ECommerce.Application.Mappers
{
    public static class DbCartMapper
    {
        public static CartDto ToCartDto(this Cart cart)
        {
            var dto = new CartDto();

            if (cart == null || cart.Items == null || !cart.Items.Any())
                return dto;

            dto.Items = cart.Items.Select(x => new CartItemDto
            {
                ProductId = x.ProductId,

                ProductName = string.Empty,
                ImageUrl = null,

                Price = x.PriceAtTime,
                Quantity = x.Quantity
            }).ToList();

            return dto;
        }
    }
}
