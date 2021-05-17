using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.UnitTests.Factories
{
    public class OrderItemsFactory
    {
        private int count = 0;
        public OrderItem InstantiateNew()
        {
            return new OrderItem()
            {
                Price = count,
                Quantity = 2  
            };
        }

        public List<OrderItem> InstantiateMany(int count)
        {
            List<OrderItem> items = new();
            for(int i=0;i<count;i++)
            {
                items.Add(InstantiateNew());
            }
            return items;
        }
    }
}