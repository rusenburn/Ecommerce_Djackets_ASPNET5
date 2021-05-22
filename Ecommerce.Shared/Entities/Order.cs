using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Shared.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [MaxLength(100)]
        public string Address { get; set; }

        [Required]
        [MaxLength(100)]
        public string ZipCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Place { get; set; }

        [Required]
        [MaxLength(100)]
        public string Phone { get; set; }

        [Required]
        public decimal PaidAmount { get; set; }

        [Required]
        [MaxLength(200)]
        public string StripeToken { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; }
        [MaxLength(300)]
        public string RecipeURL { get; set; }
        public ShippingInfo ShippingInfo { get; set; }
        
        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

    }
}