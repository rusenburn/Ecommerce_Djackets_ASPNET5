using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories;
using Ecommerce.UnitTests.Factories;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.RepositoriesTests
{
    public class RepositoryBaseTests : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseFixture Fixture { get; }
        public RepositoryBaseTests(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }


        [Fact]
        public async Task GetOne_WhenPassingValidArguments_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))

                using (var categories = new ConcreteClass(context))
                {
                    // Arrange
                    int id = 1;
                    // Act
                    var category = await categories.GetOneAsync(id);

                    // Assert
                    Assert.NotNull(category);
                    Assert.Equal(category.Id, id);
                }
            }
        }

        [Fact]
        public async Task GetOne_WhenPassingZero_ShouldReturnNull()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))

                using (var categories = new ConcreteClass(context))
                {
                    // Arrange
                    int id = 0;
                    // Act
                    var category = await categories.GetOneAsync(id);

                    // Assert
                    Assert.Null(category);

                }
            }
        }

        [Fact]
        public async Task GetOne_WhenPassingNegativeId_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var categories = new ConcreteClass(context))
            {

                // Arrange
                int id = -1;
                const string PARMETER_NAME = "id";
                // Act
                Func<Task<dynamic>> function = async () => await categories.GetOneAsync(id);

                // Assert
                await Assert.ThrowsAsync<ArgumentException>(PARMETER_NAME, function);
            }
        }

        [Fact]
        public async Task GetOne_ChangingResult_ShouldNotModefy()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                var entity = await repo.GetOneAsync(1);
                entity.Name = "EntityNewName";

                int changesCount = await context.SaveChangesAsync();

                Assert.Equal(0, changesCount);
            }
        }

        [Fact]
        public async Task GetAll_ShouldReturnAll()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {

                // Arrange
                int expectedCount = 2;
                // Act
                var categories = await repo.GetAllAsync();
                int actualCount = categories.Count();

                // Assert
                Assert.NotNull(categories);
                Assert.NotEmpty(categories);
                Assert.Equal(expectedCount, actualCount);
            }
        }


        [Fact]
        public async Task CreateOne_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {

                // Arrange
                var factory = new CategoryFactory();
                var category = factory.InstatiateNew();
                var expectedName = category.Name;
                var expectedSlug = category.Slug;

                // Act
                category = await repo.CreateOneAsync(category);
                await context.SaveChangesAsync();
                var categoryFromDb = await repo.GetOneAsync(category.Id);

                // Assert
                Assert.NotNull(category);
                Assert.NotNull(categoryFromDb);
                Assert.NotEqual(0, category.Id);
                Assert.Equal(category.Id, categoryFromDb.Id);
                Assert.Equal(expectedName, category.Name);
                Assert.Equal(expectedSlug, category.Slug);
            }
        }


        [Fact]
        public async Task CreateOne_WhenPassingNull_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange
                const string PARAMETER_NAME = "entity";

                // Act
                Func<Task<Category>> func = async () => await repo.CreateOneAsync(null);

                // Assert
                await Assert.ThrowsAsync<ArgumentNullException>(PARAMETER_NAME, func);
            }
        }


        [Fact]
        public async Task CreateMany_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange
                const int ENTITIES_TO_ADD = 5;
                List<Category> categoriesToAdd = new List<Category>();
                int initialCount = (await repo.GetAllAsync()).Count();
                var factory = new CategoryFactory();
                foreach (int i in Enumerable.Range(0, ENTITIES_TO_ADD))
                {
                    categoriesToAdd.Add(factory.InstatiateNew());
                }
                var expectedCount = initialCount + ENTITIES_TO_ADD;

                // Act
                await repo.CreateManyAsync(categoriesToAdd);
                await context.SaveChangesAsync();
                var entitiesFromDb = await repo.GetAllAsync();
                int actualCount = entitiesFromDb.Count();

                // Assert
                Assert.NotNull(entitiesFromDb);
                Assert.Equal(expectedCount, actualCount);
                Assert.All(categoriesToAdd, oldEntity => Assert.NotEqual(0, oldEntity.Id));
            }
        }

        [Fact]
        public async Task CreateMany_WhenPassingNull_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange
                const string PARAMETER_NAME = "entities";
                // Act
                Func<Task> func = async () => await repo.CreateManyAsync(null);
                await context.SaveChangesAsync();
                // Assert
                await Assert.ThrowsAsync<ArgumentNullException>(PARAMETER_NAME, func);
            }
        }

        [Fact]
        public async Task DeleteOne_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange
                Category categoryToDelete = (await repo.GetAllAsync()).First();

                // Act
                categoryToDelete = await repo.DeleteOneAsync(categoryToDelete.Id);
                var stillExistingEntity = await repo.GetOneAsync(categoryToDelete.Id);

                // Assert
                Assert.NotNull(stillExistingEntity);

                // Act
                await context.SaveChangesAsync();

                // Assert
                stillExistingEntity = await repo.GetOneAsync(categoryToDelete.Id);
                Assert.Null(stillExistingEntity);
            }
        }

        [Fact]
        public async Task DeleteOne_PassingNonExisting_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                var entityToDelete = (await repo.GetAllAsync()).First();
                var deletedCategory = await repo.DeleteOneAsync(entityToDelete.Id);
                await context.SaveChangesAsync();

                // Act
                Func<Task<Category>> func = async () => await repo.DeleteOneAsync(deletedCategory.Id);

                // Assert
                await Assert.ThrowsAsync<InvalidOperationException>(func);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteOne_PassingInvalidArguments_ShouldThrow(int invalidId)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                const string PARAMETER_NAME = "id";
                // Act
                Func<Task<Category>> func = async () => await repo.DeleteOneAsync(invalidId);

                // Assert
                await Assert.ThrowsAsync<ArgumentException>(PARAMETER_NAME, func);
            }
        }

        [Fact]
        public async Task DeleteMany_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                var entities = await repo.GetAllAsync();
                // Act
                await repo.DeleteManyAsync(entities);
                await context.SaveChangesAsync();
                // Assert
                entities = await repo.GetAllAsync();
                Assert.Empty(entities);
            }
        }

        [Fact]
        public async Task DeleteMany_WithoutSaving_ShouldNotSave()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                var entities = await repo.GetAllAsync();
                // Act
                await repo.DeleteManyAsync(entities);

                // Assert
                entities = await repo.GetAllAsync();
                Assert.NotEmpty(entities);
            }
        }

        [Fact]
        public async Task DeleteMany_PassingNullArgument_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                const string PARAMETER_NAME = "entities";
                // Act
                Func<Task> func = async () => await repo.DeleteManyAsync(null);

                // Assert
                await Assert.ThrowsAsync<ArgumentNullException>(PARAMETER_NAME, func);
            }
        }

        [Fact]
        public async Task DeleteMany_ListIncludesNull_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                List<Category> entities = (await repo.GetAllAsync()).ToList();
                entities.Add(null);
                // Act
                Func<Task> func = async () => await repo.DeleteManyAsync(entities);

                // Assert
                await Assert.ThrowsAsync<ArgumentException>(func);
            }
        }

        [Fact]
        public async Task UpdateOne_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                var categoryInDb = (await repo.GetAllAsync()).First();
                string oldName = categoryInDb.Name;
                string oldslug = categoryInDb.Slug;
                string expectedName = oldName + "Extra";
                string expectedSlug = oldslug + "Extra";
                categoryInDb.Name = expectedName;
                categoryInDb.Slug = expectedSlug;

                // Act
                var updatedEntity = await repo.UpdateOneAsync(categoryInDb.Id, categoryInDb);
                await context.SaveChangesAsync();

                // Assert
                Assert.NotNull(updatedEntity);
                Assert.Equal(expectedName, updatedEntity.Name);
                Assert.Equal(expectedSlug, updatedEntity.Slug);
            }
        }

        [Fact]
        public async Task UpdateOne_InsertingDTO_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                var categoryInDb = (await repo.GetAllAsync()).First();
                string oldName = categoryInDb.Name;
                string oldslug = categoryInDb.Slug;
                string expectedSlug = oldslug;

                string expectedName = oldName + "Extra";
                categoryInDb.Name = expectedName;
                object dto = new { Name = expectedName };

                // Act
                var updatedEntity = await repo.UpdateOneAsync<object>(categoryInDb.Id, dto);
                await context.SaveChangesAsync();

                // Assert
                Assert.NotNull(updatedEntity);
                Assert.Equal(expectedName, updatedEntity.Name);
                Assert.Equal(expectedSlug, updatedEntity.Slug);
            }
        }

        [Fact]
        public async Task UpdateOne_PassingNull_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 

                // Act
                Func<Task> func = async () => await repo.UpdateOneAsync<object>(0, null);

                // Assert
                await Assert.ThrowsAsync<ArgumentNullException>(func);
            }
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task UpdateOne_PassingInvalidArguments_ShouldThrow(int invalidId)
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                var factory = new CategoryFactory();

                // Act
                Func<Task> func = async () => await repo.UpdateOneAsync<object>(invalidId, factory.InstatiateNew());

                // Assert
                await Assert.ThrowsAsync<ArgumentException>(func);
            }
        }

        [Fact]
        public async Task UpdateOne_PassingNonExistantId_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange 
                var factory = new CategoryFactory();

                // Act
                Func<Task> func = async () => await repo.UpdateOneAsync<object>(int.MaxValue, factory.InstatiateNew());

                // Assert
                await Assert.ThrowsAsync<InvalidOperationException>(func);
            }
        }

        [Fact]
        public async Task Find_ShouldWork()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Arrange
                var allEntities = (await repo.GetAllAsync());
                var expectedFullCount = allEntities.Count();
                // Act
                var actualFullEntities = await repo.FindAsync(x => true);
                var actualFullCount = actualFullEntities.Count();

                var actualEmptyEntities = await repo.FindAsync(x => false);
                // Assert
                Assert.NotNull(actualFullEntities);
                Assert.Equal(expectedFullCount, actualFullCount);

                Assert.NotNull(actualEmptyEntities);
                Assert.Empty(actualEmptyEntities);
            }
        }

        [Fact]
        public async Task Find_PassingNull_ShouldThrow()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            using (var context = Fixture.CreateContext(transaction))
            using (var repo = new ConcreteClass(context))
            {
                // Act
                Func<Task> func = async () => await repo.FindAsync(null);

                // Assert
                await Assert.ThrowsAsync<ArgumentNullException>(func);
            }
        }
        private class ConcreteClass : RepositoryBase<Category>
        {
            public ConcreteClass(ApplicationDbContext context)
                : base(context)
            {

            }
        }
    }
}