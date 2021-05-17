using Ecommerce.Shared.Database;
using Ecommerce.Shared.Repositories;

namespace Ecommerce.UnitTests.Factories
{
    public class ProductRepositoryFactory
    {
        public ProductRepository InstatiateNew(ApplicationDbContext context)
        {
            return new ProductRepository(context);
        }
    }
}