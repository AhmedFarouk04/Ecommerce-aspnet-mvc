using System;

namespace ECommerce.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IOrderRepository Orders { get; }
        IWishlistRepository Wishlists { get; }
        IRatingRepository Ratings { get; }
        IReviewRepository Reviews { get; }
        IAdminActivityLogRepository AdminActivityLogs { get; }

        ICartRepository Carts { get; }
        void BeginTransaction();
        void Commit();
        void Rollback();
        int Complete();
    }
}
