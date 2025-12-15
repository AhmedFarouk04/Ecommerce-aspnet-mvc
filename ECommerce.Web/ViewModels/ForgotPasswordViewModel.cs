using System.ComponentModel.DataAnnotations;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; }
}
