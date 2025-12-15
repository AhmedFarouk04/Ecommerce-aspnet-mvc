using System.Collections.Generic;

namespace ECommerce.Application.DTOs
{
    public class ProductSearchResult
    {
        public IEnumerable<ProductSearchResultItem> Products { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages =>
            PageSize == 0 ? 0 : (int)System.Math.Ceiling(TotalCount / (double)PageSize);
    }
}
