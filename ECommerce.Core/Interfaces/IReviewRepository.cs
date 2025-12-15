using System.Collections.Generic;
using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        IEnumerable<Review> GetProductReviews(int productId);
        IEnumerable<Review> GetUserReviews(string userId);

        void Update(Review review);
    }
}
