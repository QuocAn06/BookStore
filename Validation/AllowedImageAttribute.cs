using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BookStore.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class AllowedImageAttribute : ValidationAttribute
    {
        public AllowedImageAttribute()
        {
            ErrorMessage = "The uploaded image is not valid.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            return ImageUploadValidator.Validate(file);
        }
    }
}