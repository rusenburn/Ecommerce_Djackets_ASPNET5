using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.Shared.Services.ServiceInterfaces;

namespace Ecommerce.Shared.Services
{
    public class OrderHelperService : IOrderHelperService
    {

        private readonly IUnitOfWork _repo;
        private readonly IPaymentService _paymentService;

        public OrderHelperService(IUnitOfWork repo, IPaymentService paymentService)
        {
            _repo = repo;
            _paymentService = paymentService;
        }

        public async Task<Order> FixOrderPriceAsync(Order order)
        {
            await FixOrderItemsPriceAsync(order);
            order.PaidAmount = order.OrderItems.Sum(oi => oi.Price);
            return order;
        }

        public async Task<Order> HandleOrderAsync(Order order)
        {
            order = await FixOrderPriceAsync(order);
            order = await _paymentService.CreateChargeAsync(order);
            if (order is null) return null;

            ICollection<OrderItem> orderItems = await SubmitOrderToDb(order);
            await SubmitOrderItemsToDb(orderItems,order.Id);
            return order;
        }

        private async Task FixOrderItemsPriceAsync(Order order)
        {

            OrderItem[] orderItems = order.OrderItems.OrderBy(oi => oi.Product.Id).ToArray();
            IEnumerable<int> productIds = orderItems.Select(oi => oi.Product.Id);
            Product[] productsFromDb = (await _repo.Products.FindAsync(p => productIds.Contains(p.Id))).OrderBy(p => p.Id).ToArray();

            // Each OrderItem Should represent 1 Unique Product , Their counts should be equal 
            if (productsFromDb.Length != productsFromDb.Length)
            {
                throw new Exception();
            }

            foreach (var orderItem in orderItems)
            {
                Product productInDb = productsFromDb.FirstOrDefault(p => p.Id == orderItem.Product.Id);
                if (productInDb is null) throw new Exception();
                if (productInDb.Price > 0)
                {
                    if (orderItem.Quantity < 0) throw new ArgumentException("orderItems cannot have a quantity with negative value!");
                    decimal price = productInDb.Price * orderItem.Quantity;
                    orderItem.Price = price;
                }
            }
        }

        private async Task<ICollection<OrderItem>> SubmitOrderToDb(Order order)
        {
            ICollection<OrderItem> orderItems = order.OrderItems;
            order.OrderItems = null;
            order = await _repo.Orders.CreateOneAsync(order);
            await _repo.SaveChangesAsync();
            return orderItems;
        }
        private async Task SubmitOrderItemsToDb(ICollection<OrderItem> orderItems,int orderId)
        {
            foreach (var item in orderItems)
            {
                item.OrderId = orderId;
                // Avoids some crazy error with entityframework
                item.Product = null;
            }
            await _repo.OrderItems.CreateManyAsync(orderItems);
            await _repo.SaveChangesAsync();
        }
    }
}