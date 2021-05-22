using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.RepositoriesTests
{
    public class ShippingRepositoryTests : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseFixture Fixure { get; }
        public ShippingRepositoryTests(SharedDatabaseFixture fixture)
        {
            Fixure = fixture;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetShippingInfoByOrderId_When_Passing_0_or_Negative_ShouldThrowArgumentException(int orderId)
        {
            using (var transaction = Fixure.Connection.BeginTransaction())
            using (var context = Fixure.CreateContext(transaction))
            using (var ShippingInfoRepository = new ShippingInfoRepository(context))
            {
                // Act
                Func<Task<ShippingInfo>> func = async() =>  await ShippingInfoRepository.GetShippingInfoByOrderId(orderId);

                // Assert
                await Assert.ThrowsAsync<ArgumentException>(func);
            }
        }

        [Fact]
        public async Task GetShippingInfoByOrderId_Should_ReturnAShippingInfo()
        {
            using (var transaction = Fixure.Connection.BeginTransaction())
            using (var context = Fixure.CreateContext(transaction))
            using (var ShippingInfoRepository = new ShippingInfoRepository(context))
            {
                // Act
                ShippingInfo shippingInfo = await ShippingInfoRepository.GetShippingInfoByOrderId(1);

                // Assert
                Assert.NotNull(shippingInfo);
            }
        }

        [Fact]
        public async Task GetShippngInfoByOrderId_ShouldReturnNull()
        {
            using (var transaction = Fixure.Connection.BeginTransaction())
            using (var context = Fixure.CreateContext(transaction))
            using (var ShippingInfoRepository = new ShippingInfoRepository(context))
            {
                // Act
                ShippingInfo nullShippingInfo = await ShippingInfoRepository.GetShippingInfoByOrderId(10);

                // Assert
                Assert.Null(nullShippingInfo);
            }
        }
    }
}