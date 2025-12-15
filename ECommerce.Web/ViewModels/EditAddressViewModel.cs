using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels
{
    public class EditAddressViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Full name must be between 5 and 100 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z\s\-']+$",
            ErrorMessage = "Full name can only contain letters (English or Arabic), spaces, hyphens, or apostrophes.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your shipping address.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Address must be at least 10 characters to be clear and complete.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9\s\.,#-\/]+$",
            ErrorMessage = "Address can only contain letters, numbers, spaces, and common symbols like . , # - /")]
        [Display(Name = "Shipping Address")]
        public string Address { get; set; }
    }
}