using Ecommerce.Shared.Entities;

namespace Ecommerce.APIUI.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; } = "";
        
        public string Thumbnail { get; set; } = "";

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}