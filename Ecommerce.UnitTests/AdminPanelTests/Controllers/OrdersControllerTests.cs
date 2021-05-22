using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using AutoMapper;
using Ecommerce.AdminPanel.Controllers;
using Ecommerce.AdminPanel.Models;
using Ecommerce.APIUI.Dtos;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Enums;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Ecommerce.UnitTests.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace Ecommerce.UnitTests.AdminPanelTests.Controllers
{
    public abstract class OrdersControllerTests : IDisposable
    {
        private bool diposed = false;
        protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
        protected Mock<IOrderRepository> OrdersRepoMock { get; }
        protected Mock<IShippingInfoRepository> ShippingInfoMock { get; }
        protected Mock<ILogger<OrdersController>> LoggerMock { get; }
        protected AutoMock Mock { get; }
        protected OrdersController ServiceUnderTest { get; }
        public OrdersControllerTests()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            LoggerMock = new Mock<ILogger<OrdersController>>();
            OrdersRepoMock = new Mock<IOrderRepository>();
            ShippingInfoMock = new Mock<IShippingInfoRepository>();
            UnitOfWorkMock.Setup(x => x.Orders)
                .Returns(OrdersRepoMock.Object);
            UnitOfWorkMock.Setup(x => x.ShippingInfoSet)
                .Returns(ShippingInfoMock.Object);
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
                var ordersFactory = new OrdersFactory();
                var orders = ordersFactory.InstantiateMany(3);

                OrdersRepoMock
                    .Setup(x => x.GetOrdersWithOrderItemsAndShippingInfoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Expression<Func<Order, bool>>>()))
                    .ReturnsAsync(orders);

                // Act
                var result = await ServiceUnderTest.Index(null);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                var viewModel = Assert.IsType<OrderIndexViewModel>(view.Model);
                Assert.Same(orders, viewModel.Orders);

            }

            [Fact]
            public async Task Returns_ViewResult_with_FilterParams_in_it_when_passed_filterParams()
            {
                // Arrange
                var filterParams = new OrdersIndexFilterParams();
                var orderFactory = new OrdersFactory();
                var orders = orderFactory.InstantiateMany(3);
                OrdersRepoMock.Setup(x => x.GetOrdersWithOrderItemsAndShippingInfoAsync(filterParams.Page, filterParams.PageSize, filterParams.Search, It.IsAny<Expression<Func<Order, bool>>>()))
                    .ReturnsAsync(orders);

                // Act
                var result = await ServiceUnderTest.Index(filterParams);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                var viewModel = Assert.IsType<OrderIndexViewModel>(view.Model);
                Assert.Same(orders, viewModel.Orders);
                Assert.Same(filterParams, viewModel.FilterParams);
            }

            [Fact]
            public async Task Returns_StatusCodeResult_500_when_exception_is_caught()
            {
                // Arrange
                OrdersRepoMock
                .Setup(x => x.GetOrdersWithOrderItemsAndShippingInfoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Expression<Func<Order, bool>>>()))
                .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.Index(null);
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }

            [Fact]
            public async Task Returns_ViewResult_and_should_be_filtering_using_ShippingInfoFilter_intead_of_getting_all()
            {
                // Arrange
                var filterParams = new OrdersIndexFilterParams();
                var orderFactory = new OrdersFactory();
                var expectedOrders = orderFactory.InstantiateMany(3);
                filterParams.Shipping = ShippingInfoFilter.NotShipped;
                OrdersRepoMock
                    .Setup(x => x.GetOrdersWithOrderItemsAndShippingInfoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Expression<Func<Order, bool>>>()))
                    .Verifiable();

                OrdersRepoMock
                    .Setup(x => x.GetOrdersByShippingInfoFilter(
                        filterParams.Shipping,
                        filterParams.Page,
                        filterParams.PageSize,
                        filterParams.Search,
                        It.IsAny<Expression<Func<Order, bool>>>()))
                    .ReturnsAsync(expectedOrders)
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.Index(filterParams);

                // Assert
                var view = Assert.IsType<ViewResult>(result);
                var actualModel = Assert.IsType<OrderIndexViewModel>(view.Model);
                Assert.Same(expectedOrders, actualModel.Orders);
                Assert.Same(filterParams, actualModel.FilterParams);
                OrdersRepoMock.Verify(x => x.GetOrdersWithOrderItemsAndShippingInfoAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<Expression<Func<Order, bool>>>()),
                        Times.Never);

                OrdersRepoMock.Verify(x => x.GetOrdersByShippingInfoFilter(filterParams.Shipping,
                        filterParams.Page,
                        filterParams.PageSize,
                        filterParams.Search,
                        It.IsAny<Expression<Func<Order, bool>>>()),
                        Times.Once);
            }
        }

        public class Details : OrdersControllerTests
        {

        }

        public class MarkAsShipped : OrdersControllerTests
        {
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task Return_NotFoundResult_when_passing_orderId_0_or_negative(int orderId)
            {
                // Arrange
                OrdersRepoMock.Setup(x => x.GetOneAsync(It.IsAny<int>())).Verifiable();
                // Act
                var result = await ServiceUnderTest.MarkAsShipped(orderId);

                // Assert
                Assert.IsType<NotFoundResult>(result);
                OrdersRepoMock.Verify(x => x.GetOneAsync(It.IsAny<int>()), Times.Never);
            }

            [Fact]
            public async Task Return_NotFoundResult_when_Order_not_found()
            {
                // Arrange
                const int orderId = 5;
                OrdersRepoMock.Setup(x => x.GetOneAsync(orderId))
                    .ReturnsAsync(default(Order));

                // Act
                var result = await ServiceUnderTest.MarkAsShipped(orderId);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task Returns_BadRequestResult_if_ShippingInfo_already_exist_for_Order()
            {
                // Arrange
                const int orderId = 5;
                var shippingInfoFactory = new ShippingInfoFactory();
                var shippingInfoInstance = shippingInfoFactory.InstantiateNew();

                var orderFactory = new OrdersFactory();
                var orderInstance = orderFactory.InstantiateNew();

                OrdersRepoMock.Setup(x => x.GetOneAsync(orderId))
                    .ReturnsAsync(orderInstance);

                ShippingInfoMock.Setup(x => x.GetShippingInfoByOrderId(orderId))
                    .ReturnsAsync(shippingInfoInstance);

                // Act
                var result = await ServiceUnderTest.MarkAsShipped(orderId);

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }

            [Fact]
            public async Task Returns_RedirectToActionResult_if_worked_correctly()
            {
                // Arrange
                const int orderId = 5;
                var shippingInfoFactory = new ShippingInfoFactory();
                var expectedShippingInfo = shippingInfoFactory.InstantiateNew();

                var orderFactory = new OrdersFactory();
                var orderInstance = orderFactory.InstantiateNew();

                OrdersRepoMock.Setup(x => x.GetOneAsync(orderId))
                    .ReturnsAsync(orderInstance);

                ShippingInfoMock.Setup(x => x.GetShippingInfoByOrderId(orderId))
                    .ReturnsAsync(default(ShippingInfo));

                ShippingInfoMock.Setup(x => x.CreateOneAsync(It.IsAny<ShippingInfo>()))
                    .Returns<ShippingInfo>(s =>
                    {
                        var shippingInfo = shippingInfoFactory.InstantiateNew();
                        shippingInfo.OrderId = s.OrderId;
                        return Task.FromResult(shippingInfo);
                    })
                    .Verifiable();

                UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).Verifiable();

                // Act
                var result = await ServiceUnderTest.MarkAsShipped(orderId);

                // Assert
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                ShippingInfoMock.Verify(x => x.CreateOneAsync(It.IsAny<ShippingInfo>()), Times.Once);
                UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);

                // get routevalue of the shipping
                redirect.RouteValues.TryGetValue(nameof(OrdersIndexFilterParams.Shipping), out object actualShippingParameter);
                Assert.Equal(ShippingInfoFilter.ShippedNotArrived, actualShippingParameter);
            }

            [Fact]
            public async Task Returns_StatusCodeResult_when_exception_is_caught()
            {
                // Arrange
                const int orderId = 5;
                OrdersRepoMock.Setup(x => x.GetOneAsync(orderId))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.MarkAsShipped(orderId);

                // Assert
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }
        }

        public class MarkAsArrived : OrdersControllerTests
        {
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task Returns_NotFoundResult_when_passing_0_or_negative_orderId_value(int orderId)
            {
                // Arrange , Act
                var result = await ServiceUnderTest.MarkAsArrived(orderId);
                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task Returns_NotFoundResult_when_Order_is_not_found()
            {
                // Arrange
                const int orderId = 5;

                // Act
                var result = await ServiceUnderTest.MarkAsArrived(orderId);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Theory, MemberData(nameof(GetInvalidShippingInfo))]
            public async Task Returns_BadRequestResult_when_trying_to_change_invalid_ShippingDate(ShippingInfo invalidShippingInfo)
            {
                // Arrange
                const int orderId = 5;
                var orderFactory = new OrdersFactory();
                var order = orderFactory.InstantiateNew();
                order.Id = orderId;
                OrdersRepoMock.Setup(x => x.GetOneAsync(orderId))
                    .ReturnsAsync(order);

                ShippingInfoMock.Setup(x => x.GetShippingInfoByOrderId(orderId))
                    .ReturnsAsync(invalidShippingInfo);

                // Act
                var result = await ServiceUnderTest.MarkAsArrived(orderId);

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }

            [Fact]
            public async Task Returns_RedirectToActionResult_when_everything_is_working()
            {
                // Arrange
                const int orderId = 5;
                var orderFactory = new OrdersFactory();
                var shippingInfoFactory = new ShippingInfoFactory();

                var expectedOrder = orderFactory.InstantiateNew();
                expectedOrder.Id = orderId;
                var shippingInfo = shippingInfoFactory.InstantiateNew();
                shippingInfo.OrderId = orderId;
                shippingInfo.ArrivalDate = null;

                OrdersRepoMock.Setup(x => x.GetOneAsync(orderId))
                    .ReturnsAsync(expectedOrder);
                ShippingInfoMock.Setup(x=>x.GetShippingInfoByOrderId(orderId))
                    .ReturnsAsync(shippingInfo);
                
                ShippingInfoMock.Setup(x=>x.UpdateOneAsync(shippingInfo.Id,shippingInfo))
                    .ReturnsAsync(shippingInfo)
                    .Verifiable();
                
                UnitOfWorkMock.Setup(x=>x.SaveChangesAsync())
                    .Verifiable();
                
                // Act
                var result = await ServiceUnderTest.MarkAsArrived(orderId);

                // Assert
                Assert.IsType<RedirectToActionResult>(result);
                ShippingInfoMock.Verify(x=>x.UpdateOneAsync(shippingInfo.Id,shippingInfo),Times.Once);
                UnitOfWorkMock.Verify(x=>x.SaveChangesAsync(),Times.Once);
            }

            [Fact]
            public async Task Returns_StatusCodeResult_internalservererror_when_exception_is_caught()
            {
                // Arrange
                const int orderId = 5;
                OrdersRepoMock.Setup(x=>x.GetOneAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());
                
                // Act
                var result = await ServiceUnderTest.MarkAsArrived(orderId);

                // Assert
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError,codeResult.StatusCode);
            }

            // used for theory
            public static IEnumerable<object[]> GetInvalidShippingInfo()
            {
                List<object[]> shippingInfoSet = new List<object[]>();
                shippingInfoSet.Add(new object[] { null });

                // Not even shipped yet
                shippingInfoSet.Add(new object[] { new ShippingInfo() { ShippedDate = null } });

                // Already arrived
                shippingInfoSet.Add(new object[] { new ShippingInfo() { ShippedDate = DateTime.UtcNow, ArrivalDate = DateTime.UtcNow } });
                return shippingInfoSet;
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