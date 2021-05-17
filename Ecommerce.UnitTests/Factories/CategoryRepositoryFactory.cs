using Ecommerce.Shared.Database;
using Ecommerce.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.UnitTests.Factories
{
    public class CategoryRepositoryFactory
    {
        public CategoryRepository InstantiateNew(ApplicationDbContext context)
        {
            return new CategoryRepository(context);
        }
    }
}