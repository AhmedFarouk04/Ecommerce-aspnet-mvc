using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels
{

    public class LoginViewModel
    {
        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9@._-]+$")]
        public string LoginInput { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }


}
