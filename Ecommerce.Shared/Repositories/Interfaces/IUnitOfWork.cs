using System;
using System.Threading.Tasks;

namespace Ecommerce.Shared.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories {get;}
        IOrderRepository Orders {get;}
        IOrderItemRepository OrderItems {get;}
        IShippingInfoRepository ShippingInfoSet {get;}
        Task<int> SaveChangesAsync();
    }
}