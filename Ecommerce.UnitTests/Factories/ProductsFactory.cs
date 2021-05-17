using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.UnitTests.Factories
{
    public class ProductsFactory
    {
        private int count = 0;
        public Product InstatiateNew()
        {
            count++;
            return new Product()
            {
                // Id = count,
                Name = $"Product{count}",
                Slug = $"ProductSlug{count}"
            };
        }
        public List<Product> InstatiateMany(int count)
        {
            List<Product> products = new List<Product>();
            for(int i=0;i<count;i++)
            {
                products.Add(InstatiateNew());
            }
            return products;
        }
    }
}