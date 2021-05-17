using System.Collections.Generic;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Validations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.AdminPanel.ViewModels
{
    public class ProductForm
    {
        public Product Product { get; set; }
        public IEnumerable<Category> Categories { get; set; }

        [MaxFileSize(1L *1024*1024)]
        [AllowImagesOnly]
        public IFormFile imageFile { get; set; }
    }
}