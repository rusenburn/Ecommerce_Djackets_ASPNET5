using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Enums;

namespace Ecommerce.Shared.Repositories.Interfaces
{
    public interface IShippingInfoRepository : IRepositoryBase<ShippingInfo>
    {
        Task<ShippingInfo> GetShippingInfoByOrderId(int orderId);
        Task<IEnumerable<ShippingInfo>> GetShippingInfoSetWithOrders(ShippingInfoFilter filter, string query = "", int pageIndex = 1, int pageSize = 20);
    }
}