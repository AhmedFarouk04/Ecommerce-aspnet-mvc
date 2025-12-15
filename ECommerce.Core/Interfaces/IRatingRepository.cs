using ECommerce.Core.Entities;
using System.Collections.Generic;

namespace ECommerce.Core.Interfaces
{
    public interface IRatingRepository : IGenericRepository<Rating>
    {
        Rating? GetUserRating(string userId, int productId);
        IEnumerable<Rating> GetProductRatings(int productId);
        double GetAverageRating(int productId);
        void AddOrUpdate(Rating rating);
    }
}
