using Ecommerce.Shared.Entities;

namespace Ecommerce.APIUI.Dtos
{
    public class OrderItemDto
    {
        public int Quantity { get; set; }
        public Product Product { get; set; }
        public decimal Price { get; set; }
    }
}