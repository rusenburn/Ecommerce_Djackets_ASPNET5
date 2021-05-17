using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.APIUI.Controllers;
using Ecommerce.APIUI.Dtos;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.APIUITests
{
    public abstract class CategoriesControllerTest
    {
        protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
        protected Mock<ILogger<CategoriesController>> Logger { get; }
        protected Mock<IMapper> MapperMock { get; }
        protected CategoriesController ServiceUnderTest { get; }
        protected Mock<ICategoryRepository> CategoryRepoMock { get; }
        protected Mock<IProductRepository> ProductRepoMock { get; }

        public CategoriesControllerTest()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            Logger = new Mock<ILogger<CategoriesController>>();
            MapperMock = new Mock<IMapper>();
            ServiceUnderTest = new CategoriesController(UnitOfWorkMock.Object, Logger.Object, MapperMock.Object);
            CategoryRepoMock = new Mock<ICategoryRepository>();
            ProductRepoMock = new Mock<IProductRepository>();
            UnitOfWorkMock
                .Setup(x => x.Categories)
                .Returns(CategoryRepoMock.Object);
            UnitOfWorkMock
                .Setup(x => x.Products)
                .Returns(ProductRepoMock.Object);
        }

        public class Get : CategoriesControllerTest
        {
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task Should_return_BadRequestObjectResult_when_passing_id_with_wrong_values(int id)
            {
                // Act
                var result = await ServiceUnderTest.Get(id);

                // Assert
                var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task Should_return_NotFoundResult_when_passing_unexisted_category_id()
            {
                // Arrange
                const int expectedId = 1;
                CategoryRepoMock
                    .Setup(x => x.GetOneAsync(expectedId))
                    .ReturnsAsync(default(Category));

                // Act
                var result = await ServiceUnderTest.Get(expectedId);

                // Assert
                var objectResult = Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task Should_return_OkObjectResult_with_Category_with_empty_products_if_category_found_but_not_products()
            {
                // Arrange
                const int expectedId = 1;
                Category expectedCategory = new Category() { Id = expectedId };
                List<Product> expectedProducts = new List<Product> { new Product() { Id = 1 } };
                CategoryRepoMock
                    .Setup(x => x.GetOneAsync(expectedId))
                    .ReturnsAsync(expectedCategory);

                ProductRepoMock
                    .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                    .ReturnsAsync(expectedProducts);
                MapperMock.Setup(x => x.Map<Category, CategoryDTO>(expectedCategory))
                    .Returns<Category>(x =>
                    {
                        var dto = new CategoryDTO();
                        dto.Id = x.Id;
                        dto.Name = x.Name;
                        dto.Slug = x.Slug;
                        dto.Products = (from p in x.Products select new ProductDto() { Name = p.Name, Slug = p.Slug }).ToList();
                        return dto;
                    });

                // Act
                var result = await ServiceUnderTest.Get(expectedId);

                // Assert
                var ObjectResult = Assert.IsType<OkObjectResult>(result);
                var actualCategory = Assert.IsType<CategoryDTO>(ObjectResult.Value);
                Assert.NotNull(actualCategory);
                Assert.Equal(expectedCategory.Name, actualCategory.Name);
                Assert.Equal(expectedCategory.Slug, actualCategory.Slug);
                Assert.Equal(expectedCategory.Id, actualCategory.Id);
                Assert.Equal(expectedCategory.Products.Count(), actualCategory.Products.Count());
            }

            [Fact]
            public async Task Should_return_StatusCodeResult500_when_error_happens()
            {
                // Arrange
                CategoryRepoMock
                    .Setup(x => x.GetOneAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.Get(1);

                // Assert
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }
        }

        public class GetCategoryBySlug : CategoriesControllerTest
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task Should_return_BadRequestObjectResult_when_passing_emtpyornull_category_slug(string categorySlug)
            {
                // Act
                var result = await ServiceUnderTest.GetCategoryBySlug(categorySlug);

                // Assert
                var ObjectResult = Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task Should_return_NotFoundResult_when_passing_nonexisted_categorySlug()
            {
                // Arrange
                const string unexistedSlug = "AnySlug";
                CategoryRepoMock.Setup(x => x.GetCategoryBySlug(unexistedSlug))
                    .ReturnsAsync(default(Category))
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.GetCategoryBySlug(unexistedSlug);

                // Assert
                Assert.IsType<NotFoundResult>(result);
                CategoryRepoMock.Verify(x => x.GetCategoryBySlug(unexistedSlug), Times.Once);
            }

            [Fact]
            public async Task Should_return_OkObjectResult_with_category_and_products()
            {
                // Arrange
                const string slug = "Slug";
                Category expectedCategory = new Category() { Name = "any", Slug = slug };
                List<Product> products = new List<Product>() { new Product { Name = "product", Category = expectedCategory } };
                var expectedDTO =new CategoryDTO();
                CategoryRepoMock.Setup(x => x.GetCategoryBySlug(slug))
                    .ReturnsAsync(expectedCategory)
                    .Verifiable();

                ProductRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                    .ReturnsAsync(products)
                    .Verifiable();
                MapperMock.Setup(x => x.Map<Category, CategoryDTO>(expectedCategory))
                    .Returns(expectedDTO);
                // Act
                var result = await ServiceUnderTest.GetCategoryBySlug(slug);

                // Assert
                var objectResult = Assert.IsType<OkObjectResult>(result);
                var actualCategory = Assert.IsType<CategoryDTO>(objectResult.Value);
                Assert.Same(expectedDTO, actualCategory);
                // Assert.Same(products, actualCategory.Products);
            }

            [Fact]
            public async Task Should_return_StatusResult500_when_error_happens()
            {
                // Arrange
                const string slug = "slug";
                CategoryRepoMock
                    .Setup(x => x.GetCategoryBySlug(slug))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.GetCategoryBySlug(slug);

                // Assert
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);

            }
        }
    }
}