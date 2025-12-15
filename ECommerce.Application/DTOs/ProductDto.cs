using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Range(1, 999999)]
        public decimal Price { get; set; }

        [Range(0, 999999)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        public bool IsInWishlist { get; set; }

        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public int? UserRating { get; set; }
        public int AvailableStock { get; set; }
    }
}
