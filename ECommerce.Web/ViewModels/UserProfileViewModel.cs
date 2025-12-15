using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using ECommerce.Web.Validations;
    using Microsoft.AspNetCore.Http;

    public class UserProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9_.-]*$",ErrorMessage = "Invalid userName format")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(120)]
        public string Email { get; set; }

        public IList<string>? Roles { get; set; }

        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp" })]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        public string? OriginalUserName { get; set; }
        public string? OriginalEmail { get; set; }
        public string? OriginalImage { get; set; }
    }

}
