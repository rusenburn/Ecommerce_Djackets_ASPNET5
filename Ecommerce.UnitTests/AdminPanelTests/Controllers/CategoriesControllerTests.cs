using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Ecommerce.AdminPanel.Controllers;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.UnitTests.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.AdminPanelTests.Controllers
{
    public abstract class CategoriesControllerTests : IDisposable
    {
        protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
        protected Mock<ILogger<CategoriesController>> LoggerMock { get; }
        protected Mock<ICategoryRepository> CategoriesRepoMock { get; }
        public Mock<IProductRepository> ProductsRepoMock { get; }
        protected AutoMock Mock { get; }
        protected CategoriesController ServiceUnderTest { get; }
        public CategoriesControllerTests()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            LoggerMock = new Mock<ILogger<CategoriesController>>();
            CategoriesRepoMock = new Mock<ICategoryRepository>();
            UnitOfWorkMock.Setup(x => x.Categories)
                .Returns(CategoriesRepoMock.Object);
            ProductsRepoMock = new Mock<IProductRepository>();
            UnitOfWorkMock.Setup(x => x.Products).Returns(ProductsRepoMock.Object);
            Mock = AutoMock.GetLoose(
                cfg =>
                {
                    cfg.RegisterMock(UnitOfWorkMock);
                    cfg.RegisterMock(LoggerMock);
                }
                );
            ServiceUnderTest = Mock.Create<CategoriesController>();
        }


        public class Index : CategoriesControllerTests
        {
            [Fact]
            public async Task Should_return_ObjectResult_with_500InternalServerError_when_Exception_is_caught()
            {
                // Arrange
                CategoriesRepoMock
                    .Setup(x => x.GetAllAsync())
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.Index();

                // Assert
                var codeResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }

            [Fact]
            public async Task Should_return_ViewResult()
            {
                // Arrange
                var factory = new CategoryFactory();
                List<Category> categories = factory.InstantiateMany(3);
                CategoriesRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(categories);

                // Act
                var result = await ServiceUnderTest.Index();

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                Assert.Same(categories, view.Model);

            }
        }

        public class Details : CategoriesControllerTests
        {
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task Should_return_NotFoundResult_when_passing_id_of_0_or_negative(int id)
            {
                // Arrange , Act
                var result = await ServiceUnderTest.Details(id);

                // Assert
                var codeResult = Assert.IsType<NotFoundResult>(result);
            }

            public async Task Should_return_NotFoundResult_when_category_was_not_found()
            {
                // Arrange
                int unexistedId = 50;
                CategoriesRepoMock
                    .Setup(x => x.GetOneAsync(unexistedId))
                    .ReturnsAsync(default(Category))
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.Details(unexistedId);

                // Assert
                var codeResult = Assert.IsType<NotFoundResult>(result);
                CategoriesRepoMock.Verify(x => x.GetOneAsync(unexistedId), Times.Once);
            }

            [Fact]
            public async Task Should_return_category_with_products()
            {
                // Arrange
                int categoryId = 5;
                var factory = new CategoryFactory();
                var productsFactory = new ProductsFactory();
                var expectedCategory = factory.InstatiateNew();
                expectedCategory.Id = categoryId;
                List<Product> products = productsFactory.InstatiateMany(3);
                CategoriesRepoMock.Setup(x => x.GetOneAsync(categoryId))
                    .ReturnsAsync(expectedCategory)
                    .Verifiable();
                ProductsRepoMock.Setup(x => x.FindAsync(p => p.CategoryId == expectedCategory.Id))
                    .ReturnsAsync(products)
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.Details(categoryId);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<Category>(view.Model);
                Assert.Same(expectedCategory, model);
                Assert.Same(products, model.Products);
                CategoriesRepoMock.Verify(x => x.GetOneAsync(categoryId), Times.Once);
                ProductsRepoMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
            }
        }
        public class UpsertGet : CategoriesControllerTests
        {
            [Fact]
            public async Task Should_return_NotFoundResult_when_passing_negative_id()
            {
                // Arrange
                int negativeId = -1;

                // Act
                var result = await ServiceUnderTest.Upsert(negativeId);

                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task Should_return_new_Category_with_id_0_when_passing_0()
            {
                // Arrange
                CategoriesRepoMock.Setup(x => x.GetOneAsync(It.IsAny<int>()))
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.Upsert(0);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                var category = Assert.IsType<Category>(view.Model);
                Assert.Equal(0, category.Id);
                CategoriesRepoMock.Verify(x => x.GetOneAsync(It.IsAny<int>()), Times.Never);
            }

            [Fact]
            public async Task Should_return_NotFoundResult_if_Category_is_not_found()
            {
                // Arrange
                int unExistedId = 5;
                CategoriesRepoMock.Setup(x => x.GetOneAsync(unExistedId))
                    .ReturnsAsync(default(Category))
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.Upsert(unExistedId);

                // Assert
                var codeResult = Assert.IsType<NotFoundResult>(result);
                CategoriesRepoMock.Verify(x => x.GetOneAsync(unExistedId), Times.Once);
            }

            [Fact]
            public async Task Should_return_existing_Category()
            {
                // Arrange
                int existedCategoryId = 5;
                var factory = new CategoryFactory();
                var expectedCategory = factory.InstatiateNew();
                expectedCategory.Id = existedCategoryId;
                CategoriesRepoMock.Setup(x => x.GetOneAsync(existedCategoryId))
                    .ReturnsAsync(expectedCategory)
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.Upsert(existedCategoryId);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<Category>(view.Model);
                Assert.Same(expectedCategory, model);
                CategoriesRepoMock.Verify(x => x.GetOneAsync(existedCategoryId), Times.Once);
            }

            [Fact]
            public async Task Should_return_StatusCodeResult_500internalServerError_if_expcetion_is_thrown()
            {
                // Arrange
                CategoriesRepoMock.Setup(x => x.GetOneAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.Upsert(3);

                // Assert
                var codeResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);

            }
        }
        public class UpsertPost : CategoriesControllerTests
        {
            [Fact]
            public async Task Should_return_Category_if_ModelState_is_invalid()
            {
                // Arrange
                var factory = new CategoryFactory();
                var category = factory.InstatiateNew();
                ServiceUnderTest.ModelState.AddModelError("Any", "Any");
                CategoriesRepoMock.Setup(x => x.CreateOneAsync(category)).Verifiable();
                CategoriesRepoMock.Setup(x => x.UpdateOneAsync(category.Id, category)).Verifiable();

                // Act
                var result = await ServiceUnderTest.Upsert(category);

                // Assert
                Assert.IsType<ViewResult>(result);
                CategoriesRepoMock.Verify(x => x.CreateOneAsync(It.IsAny<Category>()), Times.Never);
                CategoriesRepoMock.Verify(x => x.UpdateOneAsync(It.IsAny<int>(), It.IsAny<Category>()), Times.Never);
            }

            [Fact]
            public async Task Should_return_Category_with_InvalidModelState_if_inserting_category_with_non_unique_slug()
            {
                // Arrange
                var factory = new CategoryFactory();
                var category = factory.InstatiateNew();

                // This means that there is a category with that slug name in the database which makes the model invalid 
                CategoriesRepoMock.Setup(x => x.GetCategoryBySlug(category.Slug))
                    .ReturnsAsync(factory.InstatiateNew());
                CategoriesRepoMock.Setup(x => x.CreateOneAsync(It.IsAny<Category>())).Verifiable();
                CategoriesRepoMock.Setup(x => x.UpdateOneAsync(It.IsAny<int>(), It.IsAny<Category>())).Verifiable();

                // act
                var result = await ServiceUnderTest.Upsert(category);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                Assert.Same(category, view.Model);
                CategoriesRepoMock.Verify(x => x.CreateOneAsync(It.IsAny<Category>()), Times.Never);
                CategoriesRepoMock.Verify(x => x.UpdateOneAsync(It.IsAny<int>(), It.IsAny<Category>()), Times.Never);
                Assert.False(ServiceUnderTest.ModelState.IsValid);
            }

            [Fact]
            public async Task Should_create_and_save_new_Category_if_category_is_valid_and_id_is_0()
            {
                // Arrange
                var factory = new CategoryFactory();
                var newCategory = factory.InstatiateNew();
                CategoriesRepoMock.Setup(x => x.GetCategoryBySlug(newCategory.Slug))
                    .ReturnsAsync(default(Category));

                CategoriesRepoMock.Setup(x => x.CreateOneAsync(newCategory))
                    .Returns<Category>(x => { x.Id = 5; return Task.FromResult(x); })
                    .Verifiable();

                CategoriesRepoMock.Setup(x => x.UpdateOneAsync(It.IsAny<int>(), It.IsAny<Category>()))
                    .Verifiable();

                UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).Verifiable();

                // Act
                var result = await ServiceUnderTest.Upsert(newCategory);

                // Assert
                CategoriesRepoMock.Verify(x => x.CreateOneAsync(newCategory), Times.Once);
                CategoriesRepoMock.Verify(x => x.UpdateOneAsync(It.IsAny<int>(), It.IsAny<Category>()), Times.Never);
                UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            }

            [Fact]
            public async Task Should_update_and_save_modified_Category_if_Category_isValid_and_id_is_not_0()
            {
                // Arrange
                var factory = new CategoryFactory();
                var updatedCategory = factory.InstatiateNew();
                updatedCategory.Id = 5;
                CategoriesRepoMock.Setup(x => x.CreateOneAsync(It.IsAny<Category>())).Verifiable();
                CategoriesRepoMock.Setup(x => x.UpdateOneAsync(updatedCategory.Id, updatedCategory)).Verifiable();
                UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).Verifiable();

                // Act
                var result = await ServiceUnderTest.Upsert(updatedCategory);

                // Assert
                CategoriesRepoMock.Verify(x => x.CreateOneAsync(It.IsAny<Category>()), Times.Never);
                CategoriesRepoMock.Verify(x => x.UpdateOneAsync(updatedCategory.Id, updatedCategory), Times.Once);
                UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            }

            [Fact]
            public async Task Should_return_StatusCodeResult_if_exception_is_caught()
            {
                // Arrange
                var factory = new CategoryFactory();
                var category = factory.InstatiateNew();
                category.Id = 5;
                CategoriesRepoMock.Setup(x => x.UpdateOneAsync(It.IsAny<int>(), It.IsAny<Category>()))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.Upsert(category);

                // Assert
                var codeResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }
        }

        public class DeleteGet : CategoriesControllerTests
        {
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task Returns_NotFoundResult_when_passing_id_of_0_or_negative(int id)
            {
                // Act
                var result = await ServiceUnderTest.Delete(id);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task Returns_NotFoundResult_when_Category_cannot_be_found()
            {
                // Arrange
                int id = 1;
                CategoriesRepoMock.Setup(x=>x.GetOneAsync(id))
                    .ReturnsAsync(default(Category));
                
                // Act
                var result = await ServiceUnderTest.Delete(id);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task Returns_ViewResult_if_Category_is_found()
            {
                // Arrange
                int id = 1;
                var factory = new CategoryFactory();
                var expectedCategory = factory.InstatiateNew();
                CategoriesRepoMock.Setup(x=>x.GetOneAsync(id))
                    .ReturnsAsync(expectedCategory);
                // Act
                var result = await ServiceUnderTest.Delete(id);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                Assert.Same(expectedCategory,view.Model);
            }

            [Fact]
            public async Task Returns_ObjectResult_Code500_when_exception_is_caught()
            {
                // Arrange
                int id = 5;
                CategoriesRepoMock.Setup(x=>x.GetOneAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());
                
                // Act
                var result = await ServiceUnderTest.Delete(id);

                // Assert
                var codeResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError,codeResult.StatusCode);
            }
        }
        protected bool disposed = false;
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Mock.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}