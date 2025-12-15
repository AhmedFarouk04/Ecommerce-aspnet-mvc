using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces
{
    public interface IWishlistRepository : IGenericRepository<Wishlist>
    {
        Wishlist? GetItem(string userId, int productId);
        IEnumerable<Wishlist> GetUserWishlist(string userId);
        bool Exists(string userId, int productId);
    }
}
