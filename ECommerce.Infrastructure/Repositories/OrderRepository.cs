using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context) { }

        public Order GetOrderWithItems(int id)
        {
            return _dbSet
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == id);
        }

        public IEnumerable<Order> GetOrdersForUser(string userId)
        {
            return _dbSet
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ToList();
        }
        public Order GetTracked(int id)
        {
            return _dbSet.Find(id);  
        }
    }
}
