using Ecommerce.Shared.Enums;

namespace Ecommerce.AdminPanel.Models
{
    public class OrdersIndexFilterParams : FilterParams
    {
        public string UserId { get; set; }
        public ShippingInfoFilter Shipping { get; set; } = ShippingInfoFilter.All;
    }
}