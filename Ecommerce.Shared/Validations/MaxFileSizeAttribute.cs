using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Shared.Validations
{
    // https://stackoverflow.com/questions/56588900/how-to-validate-uploaded-file-in-asp-net-core
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;

        public MaxFileSizeAttribute(long maxFileSize)
        {
            _maxFileSize = maxFileSize;
            ErrorMessage = "Maximum Allowed file size is {0} bytes";
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage(file.Length));
                }
            }
            return ValidationResult.Success;
        }

        private string GetErrorMessage(long fileSize)
        {
            return string.Format(ErrorMessage,_maxFileSize,fileSize);
        }
    }
}