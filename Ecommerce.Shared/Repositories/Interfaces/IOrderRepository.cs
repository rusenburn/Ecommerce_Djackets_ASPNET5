using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;

namespace Ecommerce.Shared.Repositories.Interfaces
{
    public interface IOrderRepository : IRepositoryBase<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string id);

        Task<IEnumerable<Order>> GetOrdersWithOrderItemsAsync(int pageIndex = 1, int pageSize = int.MaxValue, string query = "", Expression<Func<Order, bool>> predicate = null);
    }
}