using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Shared.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100,MinimumLength=4)]
        public string Name { get; set; }
        public string Slug { get; set; }
        public ICollection<Product> Products { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}