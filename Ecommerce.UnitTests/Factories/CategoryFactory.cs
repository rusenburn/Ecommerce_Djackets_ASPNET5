using System.Collections.Generic;
using Ecommerce.Shared.Entities;

namespace Ecommerce.UnitTests.Factories
{
    public class CategoryFactory
    {
        private int count = 0;
        public Category InstatiateNew()
        {
            count++;
            return new Category()
            {
                // Id = count+1,
                Name = $"Spring{count}",
                Slug = $"springSlug{count}"
            };
        }
        public List<Category> InstantiateMany(int count)
        {
            List<Category> categories = new List<Category>();
            for (int i = 0; i < count; i++)
            {
                categories.Add(InstatiateNew());
            }
            return categories;
        }
    }
}