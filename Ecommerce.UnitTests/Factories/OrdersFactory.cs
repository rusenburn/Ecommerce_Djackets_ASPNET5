using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.UnitTests.Factories
{
    public class OrdersFactory
    {
        private int count = 0;
        
        public Order InstantiateNew()
        {
            count++;
            return new Order()
            {
                FirstName = $"firstName{count}",
                LastName = $"lastName{count}",
                PaidAmount = count
            };
        }
        public List<Order> InstantiateMany(int count)
        {
            List<Order> orders = new List<Order>();
            for(int i=0;i<count;i++)
            {
                orders.Add(InstantiateNew());
            }
            return orders;
        }
    }
}