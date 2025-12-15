using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {

        }
        public Category? GetWithProducts(int id)
        {
            return _context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);
        }

    }
}
