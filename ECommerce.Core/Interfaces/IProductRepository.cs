using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        IEnumerable<Product> GetProductsWithCategory();
        Product? GetProductWithCategoryById(int id);
    }
}
