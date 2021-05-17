using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.APIUI.Controllers;
using Ecommerce.APIUI.Dtos;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.APIUITests
{
    public abstract class OrdersControllerTest
    {
        protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
        protected Mock<IMapper> MapperMock { get; }
        protected Mock<IOrderHelperService> OrderHelperServiceMock { get; }
        protected Mock<ILogger<OrdersController>> LoggerMock { get; }
        protected OrdersController ServiceUnderTest { get; }
        protected string NameIdentifier { get; } = "SomeOne";

        public OrdersControllerTest()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            MapperMock = new Mock<IMapper>();
            OrderHelperServiceMock = new Mock<IOrderHelperService>();
            LoggerMock = new Mock<ILogger<OrdersController>>();
            ServiceUnderTest = new OrdersController(
                UnitOfWorkMock.Object,
                MapperMock.Object,
                OrderHelperServiceMock.Object,
                LoggerMock.Object);
            ;
            var claimPrincipal = new ClaimsPrincipal();
            claimPrincipal.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, NameIdentifier) }));
            ServiceUnderTest.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = claimPrincipal
                }
            };
        }

        public class CreatePaymentCharge : OrdersControllerTest
        {

            [Fact]
            public async Task Should_return_StatusCodeResult500_If_Something_is_wrong_when_Handling_Order()
            {
                // Arrange
                OrderInputDto dTO = new OrderInputDto() { FirstName = "First", LastName = "Last" };
                Order expectedOrder = new Order() { FirstName = "First", LastName = "Last" };
                MapperMock
                    .Setup(x => x.Map<OrderInputDto, Order>(dTO))
                    .Returns(expectedOrder);

                OrderHelperServiceMock
                    .Setup(x => x.HandleOrderAsync(expectedOrder))
                    .ThrowsAsync(new Exception());

                // Act
                var result = await ServiceUnderTest.CreatePaymentCharge(dTO);

                // Assert
                var codeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, codeResult.StatusCode);
            }

            [Fact]
            public async Task Should_return_CreatedResult_when_passing_valid_order()
            {
                // Arrange
                const string expectedFirstName = "First";
                const string expectedLastName = "last";
                OrderInputDto orderDTO = new OrderInputDto() { FirstName = expectedFirstName, LastName = expectedLastName };
                Order expectedOrder = new Order() { FirstName = expectedFirstName, LastName = expectedLastName };
                MapperMock
                    .Setup(x => x.Map<OrderInputDto, Order>(orderDTO))
                    .Returns(expectedOrder);
                OrderHelperServiceMock
                    .Setup(x => x.HandleOrderAsync(expectedOrder))
                    .Returns<Order>(x =>
                    {
                        x.PaidAmount = 5m;
                        return Task.FromResult(x);
                    });

                // Act
                var result = await ServiceUnderTest.CreatePaymentCharge(orderDTO);

                // Assert
                var objectResult = Assert.IsType<CreatedResult>(result);
                var actualOrder = Assert.IsType<Order>(objectResult.Value);
                Assert.Same(expectedOrder, actualOrder);
                Assert.Equal(NameIdentifier, actualOrder.UserId);
                Assert.Equal(5, actualOrder.PaidAmount);
            }
        }
    }
}