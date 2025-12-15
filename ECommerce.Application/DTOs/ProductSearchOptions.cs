namespace ECommerce.Application.DTOs
{
    public class ProductSearchOptions
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public double? MinRating { get; set; }

        public bool InStockOnly { get; set; }

        public string? CurrentUserId { get; set; }

        public string? Sort { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
