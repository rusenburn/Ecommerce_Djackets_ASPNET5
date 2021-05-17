using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Entities;
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

        public async Task<IEnumerable<Order>> GetOrdersWithOrderItemsAsync(int pageIndex = 1, int pageSize = int.MaxValue, string query = "", Expression<Func<Order, bool>> predicate = null)
        {
            if (pageIndex <= 0) throw new ArgumentException($"{nameof(pageIndex)} cannot have 0 or negative values", paramName: nameof(pageIndex));
            if (pageSize < 0) throw new ArgumentException($"{nameof(pageSize)} cannot have negative values", paramName: nameof(pageSize));
            if (query is null) query = "";
            // TODO fix multplying pageIndex with pageSize would give a hugeNumber
            pageSize = pageSize > 5000 ? 5000 : pageSize;
            pageIndex = pageIndex > 5000 ? 5000 : pageIndex;
            predicate = predicate ?? (o => true);
            return await Context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(io => io.Product)
                .Where(o => o.FirstName.Contains(query) || o.LastName.Contains(query) || o.Email.Contains(query))
                .Where(predicate)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}