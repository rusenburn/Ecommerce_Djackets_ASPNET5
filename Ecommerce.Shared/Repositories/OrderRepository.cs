using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Enums;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Shared.Repositories
{
    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        protected ApplicationDbContext Context { get => _context as ApplicationDbContext; }

        public OrderRepository(ApplicationDbContext context)
        : base(context)
        {
        }
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"{nameof(id)} cannot be null or empty or a white space", paramName: nameof(id));
            }
            return await (Context.Orders
                .Where(o => o.UserId == id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync());
        }

        public async Task<IEnumerable<Order>> GetOrdersWithOrderItemsAndShippingInfoAsync(int pageIndex = 1, int pageSize = 20, string query = "", Expression<Func<Order, bool>> predicate = null)
        {
            ValidateArguments(pageIndex, pageSize, ref query, out int parsedQuery);
            pageSize = pageSize > 5000 ? 5000 : pageSize;
            pageIndex = pageIndex > 5000 ? 5000 : pageIndex;
            predicate = predicate ?? (o => true);

            return await Context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(io => io.Product)
                .Include(o => o.ShippingInfo)
                .Where(o => o.FirstName.Contains(query) || o.LastName.Contains(query) || o.Email.Contains(query) || o.Id == parsedQuery)
                .Where(predicate)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByShippingInfoFilter(ShippingInfoFilter filter = ShippingInfoFilter.NotShipped, int pageIndex = 1, int pageSize = 20, string query = null, Expression<Func<Order, bool>> userPredicate = null)
        {
            ValidateArguments(pageIndex, pageSize, ref query, out int queryId);
            Expression<Func<Order, bool>> filterPredicate = (x) => true;
            Expression<Func<Order,bool>> queryPredicate = (o => o.FirstName.Contains(query) || o.LastName.Contains(query) || o.Email.Contains(query) || o.Id == queryId);
            userPredicate = userPredicate ?? (o => true);

            Expression<Func<Order, Object>> orderBy = (x) => x.CreatedAt;
            bool descending = false;
            switch (filter)
            {
                case ShippingInfoFilter.NotShipped:
                    filterPredicate = (x => x.ShippingInfo == null);
                    orderBy = x => x.CreatedAt;
                    descending = false;
                    break;
                case ShippingInfoFilter.ShippedNotArrived:
                    filterPredicate = (x => x.ShippingInfo != null && x.ShippingInfo.ArrivalDate == null);
                    orderBy = x => x.ShippingInfo.ShippedDate;
                    descending = false;
                    break;
                case ShippingInfoFilter.Arrived:
                    filterPredicate = x => x.ShippingInfo != null && x.ShippingInfo.ArrivalDate != null;
                    orderBy = x => x.ShippingInfo.ArrivalDate;
                    descending = true;
                    break;
                default:
                    filterPredicate = x => true;
                    orderBy = x => x.CreatedAt;
                    descending = true;
                    break;
            }
            
            
            IQueryable<Order> q = Context.Orders
                .Include(o => o.ShippingInfo)
                .Include(o=>o.OrderItems)
                .ThenInclude(oi=>oi.Product)
                .Where(filterPredicate)
                .Where(queryPredicate)
                .Where(userPredicate);
                
            q = descending ? q.OrderByDescending(orderBy) : q.OrderBy(orderBy);

            return await q.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        }
        private void ValidateArguments(int pageIndex, int pageSize, ref string query, out int parsedQuery)
        {
            if (pageIndex <= 0) throw new ArgumentException($"{nameof(pageIndex)} cannot have 0 or negative values", paramName: nameof(pageIndex));
            if (pageSize < 0) throw new ArgumentException($"{nameof(pageSize)} cannot have negative values", paramName: nameof(pageSize));
            if (query is null) query = "";
            int.TryParse(query, out parsedQuery);
        }

        public Task<IEnumerable<Order>> GetOrdersByShippingInfoFilter(ShippingInfoFilter filter = ShippingInfoFilter.NotShipped, int pageIndex = 1, int pageSize = 20, string query = null)
        {
            throw new NotImplementedException();
        }
    }
}