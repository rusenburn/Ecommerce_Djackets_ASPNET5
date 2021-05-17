using System.Threading.Tasks;
using Ecommerce.Shared.Entities;

namespace Ecommerce.Shared.Services.ServiceInterfaces
{
    public interface IPaymentService
    {
            Task<Order> CreateChargeAsync(Order order);
    }
}