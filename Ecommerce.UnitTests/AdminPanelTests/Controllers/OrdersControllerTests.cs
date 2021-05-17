using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using AutoMapper;
using Ecommerce.AdminPanel.Controllers;
using Ecommerce.AdminPanel.Models;
using Ecommerce.APIUI.Dtos;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Ecommerce.UnitTests.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.AdminPanelTests.Controllers
{
    public abstract class OrdersControllerTests : IDisposable
    {
        private bool diposed = false;
        protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
        protected Mock<IOrderRepository> OrdersRepoMock { get; }

        protected Mock<ILogger<OrdersController>> LoggerMock { get; }
        protected AutoMock Mock { get; }
        protected OrdersController ServiceUnderTest { get; }
        public OrdersControllerTests()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            LoggerMock = new Mock<ILogger<OrdersController>>();
            OrdersRepoMock = new Mock<IOrderRepository>();
            UnitOfWorkMock.Setup(x => x.Orders)
                .Returns(OrdersRepoMock.Object);
            Mock = AutoMock.GetLoose(cfg =>
            {
                cfg.RegisterMock(UnitOfWorkMock);
                cfg.RegisterMock(LoggerMock);
            });
            ServiceUnderTest = Mock.Create<OrdersController>();
        }
        public class Index : OrdersControllerTests
        {
            [Fact]
            public async Task Returns_ViewResult_with_Orders_when_passed_null_filterParams()
            {
                // Arrange
                const string userId = "1234";
                var ordersFactory = new OrdersFactory();
                var orders = ordersFactory.InstantiateMany(3);
                // var items = orderItemsFactory.InstantiateMany(3);

                OrdersRepoMock
                    .Setup(x => x.GetOrdersWithOrderItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Expression<Func<Order, bool>>>()))
                    .ReturnsAsync(orders);

                // Act
                var result = await ServiceUnderTest.Index(null, userId);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                var viewModel = Assert.IsType<OrderIndexViewModel>(view.Model);
                Assert.Same(orders, viewModel.Orders);
                Assert.Equal(userId, viewModel.UserId);
            }

            [Fact]
            public async Task Returns_ViewResult_with_FilterParams_in_it_when_passed_filterParams()
            {
                // Arrange
                var filterParams = new FilterParams();
                var orderFactory = new OrdersFactory();
                var orders = orderFactory.InstantiateMany(3);
                OrdersRepoMock.Setup(x=>x.GetOrdersWithOrderItemsAsync(filterParams.Page,filterParams.PageSize,filterParams.Search,It.IsAny<Expression<Func<Order, bool>>>()))
                    .ReturnsAsync(orders);
                
                // Act
                var result = await ServiceUnderTest.Index(filterParams,null);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                var viewModel = Assert.IsType<OrderIndexViewModel>(view.Model);
                Assert.Same(orders,viewModel.Orders);
                Assert.Same(filterParams,viewModel.FilterParams);
            }

            [Fact]
            public async Task Returns_StatusCodeResult_500_when_exception_is_caught()
            {
                // Arrange
                OrdersRepoMock
                .Setup(x=>x.GetOrdersWithOrderItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Expression<Func<Order, bool>>>()))
                .ThrowsAsync(new Exception());

                // Act
                var result= await ServiceUnderTest.Index(null,null);
                var codeResult =Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError,codeResult.StatusCode);
            }
        }
        private void Dispose(bool disposing)
        {
            if (!this.diposed)
            {
                if (disposing)
                {
                    Mock.Dispose();
                }
            }
            this.diposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}