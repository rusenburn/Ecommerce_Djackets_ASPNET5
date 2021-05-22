using System;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Shared.Services
{
    public class StripePaymentService : IPaymentService
    {
        public ILogger<StripePaymentService> Logger { get; }
        private readonly ILogger<StripePaymentService> _logger;
        public StripePaymentService(ILogger<StripePaymentService> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Order> CreateChargeAsync(Order order)
        {
            
            if (order is null) throw new ArgumentNullException(nameof(order));
            if (order.PaidAmount <= 0) throw new ArgumentException($"{nameof(order)} {nameof(Order.PaidAmount)}  must be bigger than 0.", paramName: nameof(order));
            
                // TODO: let the order refer to the payment type and id 
                // TODO: fill whatever you can from options
                // TODO: Add tthe ability to change currency 
                long paidAmountInCents = (long)order.PaidAmount * 100L;
                var options = new Stripe.ChargeCreateOptions()
                {
                    Source = order.StripeToken,
                    Amount = paidAmountInCents,
                    Currency = "usd",
                    Description = "Charged From Ecommerce",
                    // Extra
                    ReceiptEmail = order.Email,
                };
                
                var service = new Stripe.ChargeService();
                var charge  = await service.CreateAsync(options);
                order.RecipeURL = charge.ReceiptUrl;
                _logger.LogInformation($"a Charge with an id of {charge.Id} was created for {charge.Amount/100:C2} {charge.Currency} on {charge.ReceiptUrl}");
                return order;
        }
    }
}