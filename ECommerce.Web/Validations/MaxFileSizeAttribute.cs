using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Validations
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxBytes;

        public MaxFileSizeAttribute(int maxBytes)
        {
            _maxBytes = maxBytes;
            ErrorMessage = $"File size cannot exceed {_maxBytes / 1024 / 1024} MB.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IFormFile file || file.Length == 0)
                return ValidationResult.Success;

            if (file.Length > _maxBytes)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
