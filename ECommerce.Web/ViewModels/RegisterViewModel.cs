using ECommerce.Web.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels
{
  

    public class RegisterViewModel
    {
        [Required]
        [Remote(action: "CheckUserName", controller: "Account")]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_.-]*$",ErrorMessage = "Invalid userName format. ")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Remote(action: "CheckEmail", controller: "Account")]
        [StringLength(120)]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" })]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? ImageFile { get; set; }
    }

}

