using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        ProductSearchResult SearchProducts(ProductSearchOptions options);
        IEnumerable<ProductSearchResultItem> AutoComplete(string keyword, string? userId);
    }
}
