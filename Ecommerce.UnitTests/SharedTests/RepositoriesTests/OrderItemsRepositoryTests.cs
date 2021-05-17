using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Repositories;
using Ecommerce.UnitTests.Factories;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.RepositoriesTests
{
    public class OrderItemsRepositoryTests : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseFixture Fixture { get; }
        public OrderItemsRepositoryTests(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }


        [Fact]
        public async Task GetOrderItemsWithProductsAndCategories_AllPredeicate_ShouldReturnAll()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderItemRepository repo = new OrderItemRepository(context))
                {
                    // Arrange
                    var orderItems = await repo.GetAllAsync();
                    var expectedLength = orderItems.Count();

                    // Act
                    orderItems = await repo.GetOrderItemsWithProductsAndCategoriesAsync(o => true);
                    var actualLength = orderItems.Count();


                    // Assert
                    Assert.Equal(expectedLength, actualLength);
                }
            }
        }

        [Fact]
        public async Task GetOrderItemsWithProductsAndCategories_FalsePredicate_ShouldReturnEmpty()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderItemRepository repo = new OrderItemRepository(context))
                {
                    // Act
                    var orderItems = await repo.GetOrderItemsWithProductsAndCategoriesAsync(o => false);

                    // Assert
                    Assert.NotNull(orderItems);
                    Assert.Empty(orderItems);
                }
            }
        }

        [Fact]
        public async Task GetOrderItemsWithProductsAndCategories_ShouldIncludeProductsAndCategories()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderItemRepository repo = new OrderItemRepositoryFactory().InstantiateNew(context))
                {
                    // Act
                    var orderItems = await repo.GetOrderItemsWithProductsAndCategoriesAsync(o => true);
                    var orderItem = orderItems.First();
                    var product = orderItem.Product;
                    var category = product.Category;
                    // Assert
                    Assert.NotNull(product);
                    Assert.Equal(orderItem.ProductId, product.Id);
                    Assert.NotNull(category);
                    Assert.Equal(product.CategoryId, category.Id);
                }
            }
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetOrderItemsWithProductsAndCategories_FilteringByOrderId_ShouldGiveSameLengthAsFind(int orderId)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (OrderItemRepository repo = new OrderItemRepositoryFactory().InstantiateNew(context))
                {
                    // Arrange
                    var orderItems = await repo.FindAsync(o=>o.OrderId == orderId);
                    int expectedLength = orderItems.Count();

                    // Act
                    orderItems = await repo.GetOrderItemsWithProductsAndCategoriesAsync(o=>o.OrderId == orderId);
                    int actualLength = orderItems.Count();

                    Assert.Equal(expectedLength,actualLength);
                }
            }
        }
    }
}