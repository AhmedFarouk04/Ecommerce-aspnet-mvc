using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces
{
    public interface ICartRepository
    {
        Cart? GetByUserId(string userId);
        Cart GetOrCreate(string userId);
    }
}
