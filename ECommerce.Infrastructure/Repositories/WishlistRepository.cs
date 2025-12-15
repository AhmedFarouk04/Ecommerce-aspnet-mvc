using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Wishlist? GetItem(string userId, int productId)
        {
            return _dbSet
                .Include(w => w.Product)
                .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
        }

        public IEnumerable<Wishlist> GetUserWishlist(string userId)
        {
            return _dbSet
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToList();
        }

        public bool Exists(string userId, int productId)
        {
            return _dbSet.Any(w => w.UserId == userId && w.ProductId == productId);
        }
    }
}
