using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }
    }
}
