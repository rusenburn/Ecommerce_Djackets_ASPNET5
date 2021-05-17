// using System.Threading.Tasks;
// using Ecommerce.Shared.Database;
// using Ecommerce.Shared.Entities;
// using Ecommerce.Shared.Repositories;
// using Ecommerce.Shared.Repositories.Interfaces;
// using Microsoft.Extensions.DependencyInjection;
// using Xunit;

// namespace Ecommerce.UnitTests
// {
//     public class SomeTest : IClassFixture<CustomWebApplicationFactory<APIUI.Startup>>
//     {
//         private readonly CustomWebApplicationFactory<APIUI.Startup> _factory;
//         public SomeTest(CustomWebApplicationFactory<APIUI.Startup> factory)
//         {
//             _factory = factory;
//         }

//         [Fact]
//         public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
//         {

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 using var transaction = ctx.Database.BeginTransaction();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var product = await repo.Products.CreateOneAsync(new Product() { CategoryId = 1, Name = "product1", Slug = "product1slug" });
//                 await repo.SaveChangesAsync();
//             }

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var products = await repo.Products.FindAsync(x => x.Name == "product1");
                
//                 Assert.Empty(products);
//             }
//         }


//         [Fact]
//         public async Task Get_EndpointsReturnSuccessAndCorrectContent1()
//         {

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 using var transaction = ctx.Database.BeginTransaction();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var product = await repo.Products.CreateOneAsync(new Product() { CategoryId = 1, Name = "product1", Slug = "product1slug" });
//                 await repo.SaveChangesAsync();
//             }

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var products = await repo.Products.FindAsync(x => x.Name == "product1");
                
//                 Assert.Empty(products);
//             }
//         }

//         [Fact]
//         public async Task Get_EndpointsReturnSuccessAndCorrectContent2()
//         {

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 using var transaction = ctx.Database.BeginTransaction();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var product = await repo.Products.CreateOneAsync(new Product() { CategoryId = 1, Name = "product1", Slug = "product1slug" });
//                 await repo.SaveChangesAsync();
//             }

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var products = await repo.Products.FindAsync(x => x.Name == "product1");
                
//                 Assert.Empty(products);
//             }
//         }

//         [Fact]
//         public async Task Get_EndpointsReturnSuccessAndCorrectContent3()
//         {

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 using var transaction = ctx.Database.BeginTransaction();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var product = await repo.Products.CreateOneAsync(new Product() { CategoryId = 1, Name = "product1", Slug = "product1slug" });
//                 await repo.SaveChangesAsync();
//             }

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var products = await repo.Products.FindAsync(x => x.Name == "product1");
                
//                 Assert.Empty(products);
//             }
//         }

//         [Fact]
//         public async Task Get_EndpointsReturnSuccessAndCorrectContent4()
//         {

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 using var transaction = ctx.Database.BeginTransaction();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var product = await repo.Products.CreateOneAsync(new Product() { CategoryId = 1, Name = "product1", Slug = "product1slug" });
//                 await repo.SaveChangesAsync();
//                 await Task.Delay(1000 * 4);
//             }

//             using (var scope = _factory.Services.CreateScope())
//             {
//                 using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                 var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                 var products = await repo.Products.FindAsync(x => x.Name == "product1");
                
//                 Assert.Empty(products);
//             }
//         }
//     }
// }