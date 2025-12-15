using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IEnumerable<Product> GetProductsWithCategory()
        {
            return _dbSet
                .Include(p => p.Category)
                .AsNoTracking()
                .ToList();
        }

        public Product? GetProductWithCategoryById(int id)
        {
            return _dbSet
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id);
        }

        public override Product? GetById(int id)
        {
            return _dbSet
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id);
        }
    }
}
