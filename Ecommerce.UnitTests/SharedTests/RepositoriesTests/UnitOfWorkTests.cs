using System.Threading.Tasks;
using Ecommerce.Shared.Repositories;
using Ecommerce.UnitTests.Factories;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.RepositoriesTests
{
    public class UnitOfWorkTests : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseFixture Fixture { get; }
        public UnitOfWorkTests(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }


        [Fact]
        public async Task SaveChanges_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (UnitOfWork repo = Fixture.CreateUnitOfWork(context))
            {
                // Arrange 
                var factory = new CategoryFactory();
                var instance = factory.InstatiateNew();
                await repo.Categories.CreateOneAsync(instance);
                // Act
                int changesCount = await repo.SaveChangesAsync();
                // Assert
                Assert.NotEqual(0,changesCount);
            }
        }
    }
}