using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.Shared.Services;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.ServicesTests
{
    public class OrderHelperServiceTest
    {
        protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
        protected Mock<IPaymentService> PaymentServiceMock { get; }
        protected OrderHelperService ServiceUnderTest { get; }
        protected Mock<IProductRepository> ProductsRepoMock { get; }
        protected Mock<IOrderRepository> OrderRepoMock { get; }
        protected Mock<IOrderItemRepository> OrderItemRepoMock { get; }
        public OrderHelperServiceTest()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            PaymentServiceMock = new Mock<IPaymentService>();
            ServiceUnderTest = new OrderHelperService(UnitOfWorkMock.Object, PaymentServiceMock.Object);
            ProductsRepoMock = new Mock<IProductRepository>();
            OrderRepoMock = new Mock<IOrderRepository>();
            OrderItemRepoMock = new Mock<IOrderItemRepository>();
            UnitOfWorkMock
                .Setup(x => x.Products)
                .Returns(ProductsRepoMock.Object);
            UnitOfWorkMock
                .Setup(x => x.Orders)
                .Returns(OrderRepoMock.Object);
            UnitOfWorkMock
                .Setup(x => x.OrderItems)
                .Returns(OrderItemRepoMock.Object);
        }

        public class FixOrderPrice : OrderHelperServiceTest
        {
            [Fact]
            public async Task Should_return_Order_with_correct_PaidAmount_value()
            {
                // Arrange
                ProductsRepoMock
                    .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                    .ReturnsAsync(new Product[] { p1WithCorrectPrice, p2WithCorrectPrice });

                // Act
                var result = await ServiceUnderTest.FixOrderPriceAsync(order);

                // Assert
                Assert.Equal(expectedTotal, order.PaidAmount);
            }

            [Fact]
            public async Task Should_return_Order_with_correct_values_for_OrderItems_Price()
            {
                // Arrange
                ProductsRepoMock
                    .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                    .ReturnsAsync(new Product[] { p1WithCorrectPrice, p2WithCorrectPrice });

                // Act
                var result = await ServiceUnderTest.FixOrderPriceAsync(order);

                // Assert
                Assert.Equal(expectedTotal, order.PaidAmount);
                Assert.Equal(order.OrderItems.Take(1).First().Price, expectedOrderItem1Price);
                Assert.Equal(order.OrderItems.Skip(1).Take(1).First().Price, expectedOrderItem2Price);
            }
            //
            [Fact]
            public async Task Should_throw_Exception_when_passing_unexisted_product()
            {
                // Arrange
                ProductsRepoMock
                    .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                    .ReturnsAsync(new Product[] { p1WithCorrectPrice });

                // Act , Assert
                await Assert.ThrowsAnyAsync<Exception>(() => ServiceUnderTest.FixOrderPriceAsync(order));
            }

            [Fact]
            public async Task Should_return_null_if_payementService_returns_null()
            {
                // Arrange
                ProductsRepoMock
                    .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                    .ReturnsAsync(new Product[] { p1WithCorrectPrice, p2WithCorrectPrice });

                PaymentServiceMock
                    .Setup(x => x.CreateChargeAsync(order))
                    .ReturnsAsync(default(Order));

                UnitOfWorkMock.Setup(x => x.SaveChangesAsync())
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.HandleOrderAsync(order);

                // Assert
                Assert.Null(result);
                UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            }

            [Fact]
            public async Task Should_return_Order()
            {
                var orderItems = order.OrderItems;
                int newOrderId = 5;
                ProductsRepoMock
                    .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                    .ReturnsAsync(new Product[] { p1WithCorrectPrice, p2WithCorrectPrice });

                PaymentServiceMock
                    .Setup(x => x.CreateChargeAsync(order))
                    .ReturnsAsync(order);

                UnitOfWorkMock.Setup(x => x.SaveChangesAsync())
                    .ReturnsAsync(1)
                    .Verifiable();

                OrderRepoMock
                    .Setup(x => x.CreateOneAsync(order))
                    .ReturnsAsync(() =>
                    {
                        order.Id = newOrderId;
                        return order;
                    });

                OrderItemRepoMock
                    .Setup(x => x.CreateManyAsync(orderItems))
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.HandleOrderAsync(order);


                // Assert
                Assert.Same(order, result);
                Assert.Equal(order.Id, newOrderId);
                Assert.All(orderItems, oi => Assert.Equal(oi.OrderId, newOrderId));
                OrderItemRepoMock.Verify(x => x.CreateManyAsync(orderItems), Times.Once);
                UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
            }

            public FixOrderPrice() : base()
            {

                quantity1 = 3;
                price1 = 2.5m;
                quantity2 = 4;
                price2 = 3m;
                expectedOrderItem1Price = quantity1 * price1;
                expectedOrderItem2Price = quantity2 * price2;
                expectedTotal = quantity1 * price1 + quantity2 * price2;
                p1 = new Product() { Id = 1, Price = 1 }; // Has invalid Price
                p1WithCorrectPrice = new Product() { Id = 1, Price = price1 };
                p2 = new Product() { Id = 2, Price = 2 }; // Has invalid Price
                p2WithCorrectPrice = new Product() { Id = 2, Price = price2 };
                oi1 = new OrderItem { Product = p1, Quantity = quantity1 };
                oi2 = new OrderItem { Product = p2, Quantity = quantity2 };
                order = new Order { OrderItems = new List<OrderItem> { oi1, oi2 } };
                productIds = order.OrderItems.Select(oi => oi.Product.Id);
            }
            private int quantity1, quantity2;
            private decimal price1, price2, expectedOrderItem1Price, expectedOrderItem2Price, expectedTotal;

            private Product p1, p2, p1WithCorrectPrice, p2WithCorrectPrice;
            private OrderItem oi1, oi2;
            private Order order;
            private IEnumerable<int> productIds;
        }

        public class Example : OrderHelperServiceTest
        {
            [Fact]
            public async Task Should_return_true()
            {
                var a = await Task.FromResult(true);
                Assert.True(a);
            }
        }
    }
}