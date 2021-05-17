using System;
using System.Threading.Tasks;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Repositories.Interfaces;

namespace Ecommerce.Shared.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private bool disposed;

        public IProductRepository Products { get; private set; }
        public ICategoryRepository Categories { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IOrderItemRepository OrderItems { get; private set; }

        public UnitOfWork(
            ApplicationDbContext context,
            IProductRepository products,
            ICategoryRepository categories,
            IOrderRepository orders,
            IOrderItemRepository orderItems)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Products = products ?? throw new ArgumentNullException(nameof(products));
            Categories = categories ?? throw new ArgumentNullException(nameof(categories));
            Orders = orders ?? throw new ArgumentNullException(nameof(orders));
            OrderItems = orderItems ?? throw new ArgumentNullException(nameof(orderItems));
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}