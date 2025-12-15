using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface ICartService
    {
        CartDto GetCart(string userId);

        void Add(string userId, int productId);
        void Remove(string userId, int productId);

        void MergeGuestCart(string userId, List<CartItemDto> guestItems);
        void Clear(string userId);
        CartItemUpdateResult UpdateQuantity(string userId, int productId, int delta);
    }
}
