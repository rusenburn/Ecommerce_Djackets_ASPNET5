using System.Threading.Tasks;
using Ecommerce.Shared.Entities;

namespace Ecommerce.Shared.Services.ServiceInterfaces
{
    public interface IOrderHelperService
    {
         Task <Order> FixOrderPriceAsync(Order order);
        Task<Order> HandleOrderAsync(Order order);
    }
}