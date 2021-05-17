using System;
using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.APIUI.Dtos
{
    public class OrderInputDto
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string Place { get; set; }
        public string Phone { get; set; }
        public string StripeToken { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; }
    }

}