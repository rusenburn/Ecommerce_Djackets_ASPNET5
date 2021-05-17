using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.APIUI.Controllers;
using Ecommerce.APIUI.Dtos;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.UnitTests.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.APIUITests
{
    public abstract class ProductsControllerTest
    {
        protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
        protected Mock<IMapper> MapperMock { get; }
        protected Mock<ILogger<ProductsController>> LoggerMock { get; }
        protected ProductsController ServiceUnderTest { get; }
        protected Mock<IProductRepository> ProductsRepoMock { get; }
        protected Mock<ICategoryRepository> CategoriesRepoMock { get; }

        public ProductsControllerTest()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            MapperMock = new Mock<IMapper>();
            LoggerMock = new Mock<ILogger<ProductsController>>();
            ServiceUnderTest = new ProductsController(UnitOfWorkMock.Object,
                LoggerMock.Object,
                MapperMock.Object);
            ProductsRepoMock = new Mock<IProductRepository>();
            CategoriesRepoMock = new Mock<ICategoryRepository>();
            UnitOfWorkMock
                .Setup(x => x.Products)
                .Returns(ProductsRepoMock.Object);
            UnitOfWorkMock.Setup(x => x.Categories)
                .Returns(CategoriesRepoMock.Object);
        }

        public class GetLatestProducts : ProductsControllerTest
        {
            [Fact]
            public async Task Should_return_StatusCodeResult500_if_exception_is_thrown()
            {
                // Arrange
                var productFactory = new ProductsFactory();
                const int nonNegativeValue = 2;

                ProductsRepoMock
                    .Setup(x => x.GetLatestProducts(nonNegativeValue))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.GetLatestProducts(nonNegativeValue);

                // Assert
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }

            public async Task Should_return_OkObjectResult_if_passing_valid_positive_value()
            {
                // Arrange
                var productFactory = new ProductsFactory();
                const int num = 2;
                List<Product> products = new List<Product>(){
                    productFactory.InstatiateNew(),
                    productFactory.InstatiateNew()
                    };
                List<ProductDto> dtos = (from prod in products select new ProductDto() { Name = prod.Name, Slug = prod.Slug }).ToList();
                ProductsRepoMock
                    .Setup(x => x.GetLatestProducts(num))
                    .ReturnsAsync(products)
                    .Verifiable();
                MapperMock
                    .Setup(x => x.Map<IEnumerable<Product>, IEnumerable<ProductDto>>(products))
                    .Returns(dtos);


                // Act
                var result = await ServiceUnderTest.GetLatestProducts(num);


                // Assert
                var objectResult = Assert.IsType<OkObjectResult>(result);
                Assert.Same(dtos, objectResult.Value);
                ProductsRepoMock.Verify(x => x.GetLatestProducts(num), Times.Once);
            }

            [Fact]
            public async Task Should_return_BadRequestObjectResult_if_passed_argument_with_negative_value()
            {
                // Arrange
                const int negativeValue = -1;
                ProductsRepoMock
                    .Setup(x => x.GetLatestProducts(negativeValue))
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.GetLatestProducts(negativeValue);

                var objectResult = Assert.IsType<BadRequestObjectResult>(result);
                ProductsRepoMock.Verify(x => x.GetLatestProducts(negativeValue), Times.Never);
            }
            [Fact]
            public async Task Should_return_empty_list_if_passed_0_as_argument()
            {
                // Arrange
                const int zero = 0;
                ProductsRepoMock
                    .Setup(x => x.GetLatestProducts(zero))
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.GetLatestProducts(zero);

                // Assert
                var objectResult = Assert.IsType<OkObjectResult>(result);
                ProductsRepoMock.Verify(x => x.GetLatestProducts(zero), Times.Never);
            }
        }

        public class DetailBySlug : ProductsControllerTest
        {
            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public async Task Should_return_NotFoundResult_if_passed_empty_or_null_arguments(string invalidCategorySlug)
            {
                // Arrange
                const string productSlug = "someProduct";
                ProductsRepoMock.Setup(x => x.GetProductBySlugs(invalidCategorySlug, productSlug))
                    .Verifiable();


                // Act
                var result = await ServiceUnderTest.DetailBySlugs(invalidCategorySlug, productSlug);

                // Assert
                Assert.IsType<NotFoundResult>(result);
                ProductsRepoMock.Verify(x => x.GetProductBySlugs(invalidCategorySlug, productSlug), Times.Never);
            }

            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public async Task Should_return_NotFoundResult_if_passed_empty_or_null_productSlug(string invalidProductSlug)
            {
                // Arrange
                const string categorySlug = "someCategory";
                ProductsRepoMock.Setup(x => x.GetProductBySlugs(categorySlug, invalidProductSlug))
                    .Verifiable();
                // Act
                var result = await ServiceUnderTest.DetailBySlugs(categorySlug, invalidProductSlug);

                // Assert
                Assert.IsType<NotFoundResult>(result);
                ProductsRepoMock.Verify(x => x.GetProductBySlugs(categorySlug, invalidProductSlug), Times.Never);
            }

            [Fact]
            public async Task Should_return_NotFoundResult_if_cannot_find_Product()
            {
                // Arrage
                const string categorySlug = "someCategory";
                const string productSlug = "someProduct";

                ProductsRepoMock.Setup(x => x.GetProductBySlugs(categorySlug, productSlug))
                    .ReturnsAsync(default(Product));

                // Act
                var result = await ServiceUnderTest.DetailBySlugs(categorySlug, productSlug);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task Should_return_OkObjectResult_if_Product_is_found()
            {
                // Arrage
                const string categorySlug = "someCategory";
                const string productSlug = "someProduct";
                ProductsFactory factory = new ProductsFactory();
                var product = factory.InstatiateNew();
                var dto = new ProductDto() { Slug = productSlug };
                ProductsRepoMock.Setup(x => x.GetProductBySlugs(categorySlug, productSlug))
                    .ReturnsAsync(product)
                    .Verifiable();

                MapperMock.Setup(x => x.Map<Product, ProductDto>(product))
                    .Returns(dto);
                // Act
                var result = await ServiceUnderTest.DetailBySlugs(categorySlug, productSlug);

                // Assert
                var objectResult = Assert.IsType<OkObjectResult>(result);
                var value = Assert.IsType<ProductDto>(objectResult.Value);
                Assert.Same(dto, value);
            }

            [Fact]
            public async Task Should_return_StatusCodeResult_when_Exception_is_thrown()
            {
                // Arrange
                string categorySlug = "someSlug";
                string productSlug = "someProduct";
                ProductsRepoMock.Setup(x => x.GetProductBySlugs(categorySlug, productSlug))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.DetailBySlugs(categorySlug, productSlug);

                // Assert
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }
        }

        public class Detail : ProductsControllerTest
        {
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task Should_return_NotFoundResult_if_id_is_0_or_negative(int id)
            {
                // Act
                var result = await ServiceUnderTest.Detail(id);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task Should_return_NotFoundResult_if_Product_cannot_be_found()
            {
                // Arrange
                int id = 5;

                ProductsRepoMock.Setup(x => x.GetOneAsync(id))
                    .ReturnsAsync(default(Product));

                // Act
                var result = await ServiceUnderTest.Detail(id);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }


            [Fact]
            public async Task Should_return_OkObjectResult_with_Product()
            {
                // Arrange
                int id = 5;
                int categoryId = 1;
                var prodFactory = new ProductsFactory();
                var categoryFactory = new CategoryFactory();
                var product = prodFactory.InstatiateNew();
                var category = categoryFactory.InstatiateNew();
                product.CategoryId = categoryId;
                ProductsRepoMock.Setup(x => x.GetOneAsync(id))
                    .ReturnsAsync(product);
                CategoriesRepoMock.Setup(x => x.GetOneAsync(categoryId))
                    .ReturnsAsync(category);

                MapperMock.Setup(x => x.Map<Product, ProductDto>(product))
                    .Returns<Product>(x =>
                    {
                        var dto = new ProductDto();
                        dto.Name = product.Name;
                        dto.Category = product.Category;
                        return dto;
                    });

                // Act
                var result = await ServiceUnderTest.Detail(id);

                // Assert
                var objectResult = Assert.IsType<OkObjectResult>(result);
                var actualvalue = Assert.IsType<ProductDto>(objectResult.Value);
                Assert.Same(product.Category, actualvalue.Category);
                Assert.Equal(product.Name, actualvalue.Name);
            }

            [Fact]
            public async Task Should_return_StatusCodeResult_when_catching_Exception()
            {
                // Arrange
                int id = 1;
                ProductsRepoMock.Setup(x => x.GetOneAsync(id))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.Detail(id);

                // Assert
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }
        }
    }
}