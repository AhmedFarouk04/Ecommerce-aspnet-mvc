using ECommerce.Application.DTOs;
using SessionCartItem = ECommerce.Web.ViewModels.CartItem;

namespace ECommerce.Web.MappingProfiles
{
    public static class SessionCartMapper
    {
        public static CartDto ToCartDto(this List<SessionCartItem> sessionCart)
        {
            var dto = new CartDto();

            if (sessionCart == null || !sessionCart.Any())
                return dto;

            dto.Items = sessionCart.Select(x => new CartItemDto
            {
                ProductId = x.ProductId,
                ProductName = x.Name,
                ImageUrl = x.ImageUrl,
                Price = x.Price,
                Quantity = x.Quantity
            }).ToList();

            return dto;
        }
    }
}
