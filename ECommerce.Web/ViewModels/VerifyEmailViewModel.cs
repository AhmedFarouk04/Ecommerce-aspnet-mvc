using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels
{
    public class VerifyEmailViewModel
    {

        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "invalid Code .")]
        public string Code { get; set; }
    }
}
