using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories;
using Ecommerce.Shared.Services;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.ServicesTests
{
    public class OrderHelperServiceTests : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseFixture Fixture { get; }
        public OrderHelperServiceTests(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(4, 4, 4)]
        [InlineData(10, 5, 4)]
        [InlineData(0, 0, 0)]
        [InlineData(0, 10, 0)]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue)]
        public async Task FixOrderPrice_ShouldWork(int product1Quantity, int product2Quantity, int product3Quantity)
        {
            // TODO use moc instead
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using var context = Fixture.CreateContext(transaction);
                var repo = Fixture.CreateUnitOfWork(context);
                OrderHelperService os = new OrderHelperService(repo, null);
                var products = (await repo.Products.FindAsync(p => p.Id == 1 || p.Id == 2 || p.Id == 3)).OrderBy(p => p.Id).ToArray();


                // Arrange
                OrderItem oi1 = InstantiateOrderItem(1, product1Quantity);
                OrderItem oi2 = InstantiateOrderItem(2, product2Quantity);
                OrderItem oi3 = InstantiateOrderItem(3, product3Quantity);

                Order order = new Order();
                order.OrderItems = new List<OrderItem>();
                order.OrderItems.Add(oi1);
                order.OrderItems.Add(oi2);
                order.OrderItems.Add(oi3);

                // Act
                order = await os.FixOrderPriceAsync(order);

                // Assert
                Assert.True(oi1.Price >= 0);
                Assert.True(oi2.Price >= 0);
                Assert.True(oi3.Price >= 0);

                Assert.Equal(products[0].Price * oi1.Quantity, oi1.Price);
                Assert.Equal(products[1].Price * oi2.Quantity, oi2.Price);
                Assert.Equal(products[2].Price * oi3.Quantity, oi3.Price);

                Assert.Equal(oi1.Price + oi2.Price + oi3.Price, order.PaidAmount);
                Assert.True(order.PaidAmount >= 0);
            }
        }


        [Theory]
        [InlineData(-1, 2, 3)]
        [InlineData(1, 2, -3)]
        public async Task FixOrderPrice_InsertingNegativeQuantities_ShouldThrow(int product1Quantity, int product2Quantity, int product3Quantity)
        {
            // TODO use moc instead
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using var context = Fixture.CreateContext(transaction);
                var repo = Fixture.CreateUnitOfWork(context);
                moqPayment mocked = new moqPayment();
                OrderHelperService os = new OrderHelperService(repo, mocked);
                var products = (await repo.Products.FindAsync(p => p.Id == 1 || p.Id == 2 || p.Id == 3)).OrderBy(p => p.Id).ToArray();


                // Arrange
                OrderItem oi1 = InstantiateOrderItem(1, product1Quantity);
                OrderItem oi2 = InstantiateOrderItem(2, product2Quantity);
                OrderItem oi3 = InstantiateOrderItem(3, product3Quantity);

                Order order = new Order();
                order.OrderItems = new List<OrderItem>();
                order.OrderItems.Add(oi1);
                order.OrderItems.Add(oi2);
                order.OrderItems.Add(oi3);

                // Act
                Func<Task<Order>> func = async () => await os.FixOrderPriceAsync(order);

                // Assert
                await Assert.ThrowsAnyAsync<Exception>(func);
            }
        }


        [Fact]
        public async Task FixOrderPrice_AskingForNonExistantProduct_ShouldThrow()
        {
            using var moq = AutoMock.GetLoose();
            moq.Mock<IPaymentService>();
            // TODO use moc instead
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using var context = Fixture.CreateContext(transaction);
                var repo = Fixture.CreateUnitOfWork(context);
                var paymentService = moq.Create<IPaymentService>();
                OrderHelperService os = new OrderHelperService(repo, paymentService);

                // Arrange
                int nonExistantProductId = 500;
                OrderItem orderItem = InstantiateOrderItem(nonExistantProductId, 1);
                Order order = new Order();
                order.OrderItems = new List<OrderItem>();
                order.OrderItems.Add(orderItem);

                // Act
                Func<Task<Order>> func = async () => await os.FixOrderPriceAsync(order);

                // Assert
                await Assert.ThrowsAnyAsync<Exception>(func);
            }
        }

        private OrderItem InstantiateOrderItem(int productId, int quantity)
        {
            return new OrderItem { Product = new Product { Id = productId }, Quantity = quantity };
        }

    }
    class moqPayment : IPaymentService
    {
        public async Task<Order> CreateChargeAsync(Order order)
        {
            return await Task.FromResult(order);
        }
    }
}