using Ecommerce.Shared.Database;
using Ecommerce.Shared.Repositories;

namespace Ecommerce.UnitTests.Factories
{
    public class OrderItemRepositoryFactory
    {
        public OrderItemRepository InstantiateNew(ApplicationDbContext context)
        {
            return new OrderItemRepository(context);
        }
    }
}