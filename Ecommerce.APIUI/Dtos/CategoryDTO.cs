using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.APIUI.Dtos
{
    public class CategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Slug { get; set; }
        public ICollection<ProductDto> Products { get; set; }
    }
}