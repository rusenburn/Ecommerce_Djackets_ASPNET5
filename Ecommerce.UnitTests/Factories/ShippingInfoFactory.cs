using System;
using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.UnitTests.Factories
{
    public class ShippingInfoFactory
    {
        private int count;
        public ShippingInfo InstantiateNew()
        {
            return new ShippingInfo(){ShippedDate = DateTime.UtcNow};
        }
    }
}