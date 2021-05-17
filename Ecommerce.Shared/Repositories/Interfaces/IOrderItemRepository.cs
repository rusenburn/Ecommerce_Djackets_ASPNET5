using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;

namespace Ecommerce.Shared.Repositories.Interfaces
{
    public interface IOrderItemRepository : IRepositoryBase<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetOrderItemsWithProductsAndCategoriesAsync(Expression<Func<OrderItem, bool>> predicate);
    }
}