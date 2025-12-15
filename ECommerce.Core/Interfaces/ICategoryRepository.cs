using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {

        Category? GetWithProducts(int id);
    }
}
