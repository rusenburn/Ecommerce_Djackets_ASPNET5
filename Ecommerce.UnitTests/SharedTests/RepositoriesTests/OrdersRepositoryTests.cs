using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories;
using Ecommerce.UnitTests.Factories;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.RepositoriesTests
{
    public class OrdersRepositoryTests : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseFixture Fixture { get; }
        public OrdersRepositoryTests(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task GetOrdersByUserId_ShouldReturnList()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderRepository repo = new OrderRepository(context))
                {
                    // Arrange
                    const string USERID = "1";

                    // Act
                    var orders = await repo.GetOrdersByUserIdAsync(USERID);

                    // Assert
                    Assert.NotEmpty(orders);
                }
            }
        }

        [Fact]
        public async Task GetOrdersByUserId_ShouldIncludeOrderItems()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderRepository repo = new OrderRepositoryFactory().InstantiateNew(context))
                {
                    // Arrange
                    const string USERID = "1";

                    // Act
                    var orders = await repo.GetOrdersByUserIdAsync(USERID);
                    var orderItemsOfFirstOrder = orders.FirstOrDefault().OrderItems;

                    // Assert
                    Assert.NotEmpty(orderItemsOfFirstOrder);

                }
            }
        }

        [Fact]
        public async Task GetOrdersByUserId_ShouldIncludeProducts()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderRepository repo = new OrderRepositoryFactory().InstantiateNew(context))
                {
                    // Arrange
                    const string USERID = "1";

                    // Act
                    var orders = await repo.GetOrdersByUserIdAsync(USERID);
                    var orderItemsOfFirstOrder = orders.FirstOrDefault().OrderItems;
                    var someProduct = orderItemsOfFirstOrder.FirstOrDefault().Product;

                    // Assert
                    Assert.NotNull(someProduct);
                }
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetOrdersByUserId_PassingInvalidId_ShouldThrowError(string userId)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderRepository repo = new OrderRepositoryFactory().InstantiateNew(context))
                {
                    // Arrange
                    Func<Task<IEnumerable<Order>>> func = async () =>
                            await repo.GetOrdersByUserIdAsync(userId);

                    // Assert
                    await Assert.ThrowsAsync<ArgumentException>(func);
                }
            }
        }


        [Fact]
        public async Task GetOrderWithOrderItems_ShouldNotReturnEmpty()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderRepository repo = new OrderRepositoryFactory().InstantiateNew(context))
                {
                    // Act
                    var orders = await repo.GetOrdersWithOrderItemsAndShippingInfoAsync();

                    // Assert
                    Assert.NotEmpty(orders);
                }
            }
        }


        [Fact]
        public async Task GetOrderWithOrderItems_ShouldIncludeOrderItemsAndProducts()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderRepository repo = new OrderRepositoryFactory().InstantiateNew(context))
                {
                    // Act
                    var orders = await repo.GetOrdersWithOrderItemsAndShippingInfoAsync();
                    var orderItems = orders.First().OrderItems;
                    var orderItem = orderItems.First();
                    var product = orderItem.Product;

                    // Assert
                    Assert.NotEmpty(orderItems);
                    Assert.NotNull(orderItem);
                    Assert.NotEqual(0, orderItem.Id);
                    Assert.NotNull(product);
                    Assert.NotEqual(0, product.Id);
                }
            }
        }

        [Theory]
        [InlineData(1, 1, "")]
        [InlineData(int.MaxValue, int.MaxValue, "")]
        [InlineData(1, 1, null)]
        public async Task GetOrderWithOrderItems_Work(int pageIndex, int pageSize, string query)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderRepository repo = new OrderRepositoryFactory().InstantiateNew(context))
                {
                    // Act
                    var orders = await repo.GetOrdersWithOrderItemsAndShippingInfoAsync(pageIndex, pageSize, query, null);

                    // Assert
                    Assert.NotNull(orders);
                }
            }
        }


        [Theory]
        [InlineData(0, 1, "")]
        [InlineData(-1, 1, "")]
        [InlineData(1, -1, "")]
        public async Task GetOrderWithOrderItems_PassingInvalidArguments_ShouldThrow(int pageIndex, int pageSize, string query)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderRepository repo = new OrderRepositoryFactory().InstantiateNew(context))
                {
                    // Act
                    Func<Task<IEnumerable<Order>>> func = async () =>
                        await repo.GetOrdersWithOrderItemsAndShippingInfoAsync(pageIndex, pageSize, query, null);

                    // Assert
                    await Assert.ThrowsAsync<ArgumentException>(func);
                }
            }
        }
    }
}