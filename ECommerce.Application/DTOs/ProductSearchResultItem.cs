namespace ECommerce.Application.DTOs
{
    public class ProductSearchResultItem
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }      
        public string? ThumbnailUrl { get; set; }  

        public int Stock { get; set; }


        public int AvailableStock { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }

        public bool IsInWishlist { get; set; }
    }
}
