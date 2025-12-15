using ECommerce.Web.Validations;
using ECommerce.Web.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels
{
    

    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9\s-]+$")]
        public string Name { get; set; }

        [Range(1, 999999)]
        public decimal Price { get; set; }

        [Range(0, 999999)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool RemoveImage { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }

        public string? ExistingImageUrl { get; set; }

        [DataType(DataType.Upload)]
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp" })]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? ImageFile { get; set; }
    }


}
