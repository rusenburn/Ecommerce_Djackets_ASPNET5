using System.Collections;
using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.AdminPanel.Models
{
    public class ProductsIndexViewModel : IEnumerable<Product>
    {
        public IEnumerable<Product> Products { get; set; }
        public FilterParams FilterParams { get; set; }

        public IEnumerator<Product> GetEnumerator()
        {
            foreach (var product in Products)
            {
                yield return product; 
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}