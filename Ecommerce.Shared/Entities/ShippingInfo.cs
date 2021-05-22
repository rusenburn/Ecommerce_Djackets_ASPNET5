using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Shared.Entities
{
    public class ShippingInfo
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        
        [Required]
        public Order Order { get; set; }
        public DateTime? ShippedDate { get; set; } = null;
        public DateTime? ArrivalDate { get; set; } = null;
    }
}