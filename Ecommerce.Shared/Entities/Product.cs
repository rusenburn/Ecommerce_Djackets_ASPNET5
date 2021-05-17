using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Shared.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Slug { get; set; }

        [MaxLength(300)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName="decimal(8,2)")]
        public decimal Price { get; set; }

        public string Image { get; set; } = "";
        public string Thumbnail { get; set; } = "";
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            return $"{Name}";
        }


    }
}