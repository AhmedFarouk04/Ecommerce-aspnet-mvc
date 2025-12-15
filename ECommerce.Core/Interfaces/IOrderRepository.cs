using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Order GetOrderWithItems(int id);
        IEnumerable<Order> GetOrdersForUser(string userId);
        Order GetTracked(int id);
    }

}
