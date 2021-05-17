using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories;
using Ecommerce.UnitTests.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.RepositoriesTests
{
    public class ProductsRepositoryTests : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseFixture Fixture { get; }
        public ProductsRepositoryTests(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        async Task GetLatestProducts_Should_Work(int numberOfEntries)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {
                    // Arrange
                    var expectedProducts = (await productsRepo.GetAllAsync());
                    int productsInDbCount = expectedProducts.Count();

                    // If asked to give 3 but there is only 2 in db should return 2 , but if there is more than 3 should return only 3

                    int expectedCount = Math.Min(productsInDbCount, numberOfEntries);

                    var expectedOrdered = expectedProducts.OrderByDescending(p => p.DateAdded).ToArray();

                    // Act
                    var actualProducts = (await productsRepo.GetLatestProducts(numberOfEntries)).ToArray();

                    int actualCount = actualProducts.Count();


                    // Assert
                    Assert.Equal(expectedCount, actualCount);
                    for (int i = 0; i < expectedCount; i++)
                    {
                        var expectedItem = expectedOrdered[i];
                        var actualItem = actualProducts[i];
                        Assert.Equal(expectedItem.Id, actualItem.Id);
                        Assert.Equal(expectedItem.Name, actualItem.Name);
                        Assert.Equal(expectedItem.Slug, actualItem.Slug);
                    }
                }
            }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        async Task GetLatestProducts_Should_Not_Work(int numberOfEntries)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {
                    // Arrange
                    Func<Task<IEnumerable<Product>>> func = async () =>
                     await productsRepo.GetLatestProducts(numberOfEntries);


                    // Assert
                    await Assert.ThrowsAsync<ArgumentNullException>(func);
                }
            }
        }


        [Fact]
        async Task GetProductBySlugs_Should_Work()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {
                    // Arrange
                    var expectedProduct = (await productsRepo.GetAllAsync()).Last();
                    var expectedCategory = await context.Categories.FirstOrDefaultAsync(c=>c.Id == expectedProduct.CategoryId);
                    // Act
                    var actualProduct = await productsRepo.GetProductBySlugs(expectedCategory.Slug, expectedProduct.Slug);
                    var nonExistingProduct = await productsRepo.GetProductBySlugs(expectedCategory.Slug + "_1", expectedProduct.Slug);
                    // Assert
                    Assert.Null(nonExistingProduct);
                    Assert.NotNull(actualProduct);
                    Assert.Equal(expectedProduct.Slug, actualProduct.Slug);
                    Assert.Equal(expectedCategory.Id, actualProduct.CategoryId);
                }
            }
        }



        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        // mb useless test
        public async Task GetProductBySlugs_Should_Return_Null(string invalidName)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {
                    // Act
                    var product = await productsRepo.GetProductBySlugs(invalidName, invalidName);

                    // Assert
                    Assert.Null(product);
                }
            }
        }

        [Fact]
        public async Task GetProductsWithCategories_Should_Include_Categories()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {
                    // Arrange
                    int maxPageCount = 20;

                    // Act
                    var products = await productsRepo.GetProductsWithCategoriesAsync(1, 20, "");
                    int actualCount = products.Count();

                    // Assert
                    Assert.True(actualCount > 0);
                    Assert.All(products, p => Assert.NotNull(p.Category));
                    Assert.True(actualCount <= maxPageCount);
                }
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(20)]
        [InlineData(int.MaxValue)]
        public async Task GetProductsWithCategories_PageSize_Should_Work(int maxPageSize)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {

                    // Act
                    var productsPage1 = await productsRepo.GetProductsWithCategoriesAsync(1, maxPageSize, "");
                    var productsPage2 = await productsRepo.GetProductsWithCategoriesAsync(2, maxPageSize, "");
                    int page1Size = productsPage1.Count();
                    int page2Size = productsPage2.Count();
                    // Assert
                    Assert.True(page1Size > 0);
                    Assert.True(page1Size <= maxPageSize);
                    Assert.True(page2Size <= maxPageSize);
                }
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task GetProductsWithCategories_EmptyQuery_Should_ReturnAll(string query)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {
                    // Arrange
                    var expectedProducts = await productsRepo.GetAllAsync();
                    int expectedLength = expectedProducts.Count();
                    // Act
                    var actualProducts = await productsRepo.GetProductsWithCategoriesAsync(1, int.MaxValue, query);
                    int actualLength = actualProducts.Count();

                    // Assert
                    Assert.Equal(expectedLength, actualLength);
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task GetProductsWithCategories_ZeroOrNegativePageIndex_Should_Not_Work(int pageIndex)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {
                    // Arrange
                    Func<Task<IEnumerable<Product>>> func = async () =>
                     await productsRepo.GetProductsWithCategoriesAsync(pageIndex,int.MaxValue,""); 
                    
                    // Assert
                    await Assert.ThrowsAsync<ArgumentException>(func);
                }
            }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task GetProductsWithCategories_NegativePageSize_Should_Not_Work(int pageIndex)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (ProductRepository productsRepo = new ProductRepositoryFactory().InstatiateNew(context))
                {
                    // Arrange
                    Func<Task<IEnumerable<Product>>> func = async () =>
                     await productsRepo.GetProductsWithCategoriesAsync(1,pageIndex,""); 
                    
                    // Assert
                    await Assert.ThrowsAsync<ArgumentException>(func);
                }
            }
        }
    }
}