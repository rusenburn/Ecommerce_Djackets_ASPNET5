using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Shared.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public Order Order { get; set; }
        public int OrderId { get; set; }
        public Product Product { get; set; }
        public int ProductId { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Range(1,int.MaxValue)]
        public int Quantity { get; set; } = 1;

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}