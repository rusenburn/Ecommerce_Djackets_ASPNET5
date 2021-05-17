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
    public class OrderItemRepository : RepositoryBase<OrderItem>, IOrderItemRepository
    {
        public ApplicationDbContext Context { get => _context as ApplicationDbContext; }
        public OrderItemRepository(ApplicationDbContext context)
        : base(context)
        { }
        public async Task<IEnumerable<OrderItem>> GetOrderItemsWithProductsAndCategoriesAsync(Expression<Func<OrderItem, bool>> predicate)
        {
            return await Context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(predicate)
                .ToListAsync();
        }
    }
}