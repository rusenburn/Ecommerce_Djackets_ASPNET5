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
    public class ShippingInfoRepository : RepositoryBase<ShippingInfo>, IShippingInfoRepository
    {
        protected ApplicationDbContext Context { get => _context as ApplicationDbContext; }
        public ShippingInfoRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<ShippingInfo> GetShippingInfoByOrderId(int orderId)
        {
            if (orderId <= 0) throw new ArgumentException($"{nameof(orderId)} cannot have a value of 0 or negative \r current value is {orderId}", nameof(orderId));
            return await Context.ShippingInfoSet.FirstOrDefaultAsync(s => s.OrderId == orderId);
        }

        [Obsolete]
        public async Task<IEnumerable<ShippingInfo>> GetShippingInfoSetWithOrders(ShippingInfoFilter filter,string query="",int pageIndex=1,int pageSize=20)
        {
            Expression<Func<ShippingInfo, bool>> predicate;
            Expression<Func<ShippingInfo,Object>> orderBy ;
            switch (filter)
            {
                case ShippingInfoFilter.NotShipped:
                    predicate = (s) => s.ShippedDate == null;
                    orderBy = (s)=>s.ShippedDate;
                    break;
                case ShippingInfoFilter.ShippedNotArrived:
                    predicate = (s) => s.ShippedDate != null && s.ArrivalDate == null;
                    orderBy = (s)=>s.ArrivalDate;
                    break;
                case ShippingInfoFilter.Arrived:
                    predicate = (s) => s.ArrivalDate != null;
                    orderBy = (s)=>s.ArrivalDate;
                    break;
                default:
                    predicate = (s) => true;
                    orderBy = (s)=>s.Id;
                    break;
            }
            return await Context.ShippingInfoSet
                .Include(s=>s.Order)
                .Where(predicate)
                .OrderByDescending(orderBy)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}