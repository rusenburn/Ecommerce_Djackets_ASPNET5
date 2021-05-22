using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.AdminPanel.Models
{
    public class OrderIndexViewModel
    {
        public IEnumerable<Order> Orders { get; set; }
        public OrdersIndexFilterParams FilterParams { get; set; }
        public string UserId { get; set; }
    }
}