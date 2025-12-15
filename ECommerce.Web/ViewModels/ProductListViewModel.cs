using ECommerce.Application.DTOs;
using System.Collections.Generic;

namespace ECommerce.Web.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<ProductSearchResultItem> Products { get; set; }

        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? Sort { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinRating { get; set; }
        public bool InStockOnly { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; } = 8;
        public int TotalCount { get; set; }
        public int TotalPages =>
            PageSize == 0 ? 0 : (int)System.Math.Ceiling(TotalCount / (double)PageSize);

        public IEnumerable<CategoryDto> Categories { get; set; }
    }
}
