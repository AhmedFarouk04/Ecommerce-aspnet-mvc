using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ECommerce.Infrastructure.Repositories
{
    public class RatingRepository : GenericRepository<Rating>, IRatingRepository
    {
        public RatingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Rating? GetUserRating(string userId, int productId)
        {
            return _dbSet
                .FirstOrDefault(r => r.UserId == userId && r.ProductId == productId);
        }

        public IEnumerable<Rating> GetProductRatings(int productId)
        {
            return _dbSet
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public double GetAverageRating(int productId)
        {
            var ratings = _dbSet.Where(r => r.ProductId == productId);

            if (!ratings.Any())
                return 0;

            return ratings.Average(r => r.Stars);
        }

        public void AddOrUpdate(Rating rating)
        {
            var existing = GetUserRating(rating.UserId, rating.ProductId);

            if (existing == null)
            {
                _dbSet.Add(rating);
            }
            else
            {
                existing.Stars = rating.Stars;
                existing.Comment = rating.Comment;
                existing.UpdatedAt = DateTime.UtcNow;

                _dbSet.Update(existing);
            }
        }
    }
}
