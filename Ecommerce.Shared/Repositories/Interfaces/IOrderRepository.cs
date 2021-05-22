using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Enums;

namespace Ecommerce.Shared.Repositories.Interfaces
{
    public interface IOrderRepository : IRepositoryBase<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByShippingInfoFilter(ShippingInfoFilter filter = ShippingInfoFilter.NotShipped, int pageIndex = 1, int pageSize = 20, string query = null, Expression<Func<Order, bool>> userPredicate = null);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string id);

        Task<IEnumerable<Order>> GetOrdersWithOrderItemsAndShippingInfoAsync(int pageIndex = 1, int pageSize = int.MaxValue, string query = "", Expression<Func<Order, bool>> userPredicate = null);
    }
}