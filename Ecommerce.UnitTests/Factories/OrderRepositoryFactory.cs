using Ecommerce.Shared.Database;
using Ecommerce.Shared.Repositories;

namespace Ecommerce.UnitTests.Factories
{
    public class OrderRepositoryFactory
    {
        public OrderRepository InstantiateNew(ApplicationDbContext context)
        {
            return new OrderRepository(context);
        }
    }
}