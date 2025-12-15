using ECommerce.Application.DTOs;
using System.ComponentModel.DataAnnotations;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$",
      ErrorMessage = "Name must contain letters only")]
    public string FullName { get; set; }



    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Address { get; set; }

    public CartDto Cart { get; set; }
}
