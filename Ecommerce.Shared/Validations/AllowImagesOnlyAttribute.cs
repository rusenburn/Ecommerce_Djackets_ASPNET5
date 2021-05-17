using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Shared.Validations
{
    public class AllowImagesOnlyAttribute : ValidationAttribute
    {

        public AllowImagesOnlyAttribute()
        {
            ErrorMessage = "Only images are allowed";
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (
                    file.ContentType is null ||
                    file.ContentType.Split('/')[0].ToLower() != "image")
                {
                    return new ValidationResult(GetErrorMessage(file));
                }
            }
            return ValidationResult.Success;

        }

        private string GetErrorMessage(IFormFile file)
        {
            var contentType = file.ContentType?.Split('/')[0].ToLower();
            return string.Format(ErrorMessage,contentType);
        }
    }
}