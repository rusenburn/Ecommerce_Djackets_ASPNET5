using System.Threading.Tasks;
using Ecommerce.Shared.Repositories;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.UnitTests.Factories;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.RepositoriesTests
{
    // https://docs.microsoft.com/en-us/ef/core/testing/sharing-databases
    public class CategoryRepositoryTests : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseFixture Fixture { get; }
        public CategoryRepositoryTests(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task Should_not_update_category()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (CategoryRepository categoryRepo = new CategoryRepositoryFactory().InstantiateNew(context))
                {
                    // Arrange
                    var category = await categoryRepo.GetOneAsync(1);
                    category.Name = "SomeOtherName";

                    // Act
                    int expected = 0;
                    int changes = await context.SaveChangesAsync();

                    // Assert
                    Assert.Equal(expected, changes);
                }
            }
        }

        [Fact]
        public async Task GetCategoryBySlug_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                using (CategoryRepository categoryRepo = new CategoryRepositoryFactory().InstantiateNew(context))
                {
                    // Arrange
                    var expectedCategory = await categoryRepo.GetOneAsync(1);
                    string expectedSlugName = expectedCategory.Slug;

                    // Act
                    var ActualCategory = await categoryRepo.GetCategoryBySlug(expectedCategory.Slug);

                    Assert.NotNull(ActualCategory);
                    Assert.Equal(expectedCategory.Id, ActualCategory.Id);
                    Assert.Equal(expectedCategory.Name, ActualCategory.Name);
                    Assert.Equal(expectedCategory.Slug, ActualCategory.Slug);
                    Assert.Equal(expectedSlugName, ActualCategory.Slug);
                }
            }
        }
    }
}