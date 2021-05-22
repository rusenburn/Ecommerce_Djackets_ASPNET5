using System;
using System.Data.Common;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.UnitTests
{
    public class SharedDatabaseFixture : IDisposable
    {
        public static readonly object _lock = new object();
        private static bool _databaseInitialized;

        public SharedDatabaseFixture()
        {
            // Connection = new SqlConnection(@"Server=(localdb)\mssqllocaldb;Database=EFTestSample;ConnectRetryCount=0");
            Connection = new SqlConnection(@"Server=RPLUS-PC;Database=EFTestSample;Trusted_Connection=True;ConnectRetryCount=0");
            Seed();
            Connection.Open();
        }
        public DbConnection Connection { get; }


        public ApplicationDbContext CreateContext(DbTransaction transaction = null)
        {
            var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(Connection).Options);

            if (transaction != null)
            {
                context.Database.UseTransaction(transaction);
            }
            return context;
        }


        public UnitOfWork CreateUnitOfWork(ApplicationDbContext context)
        {
            ProductRepository products = new ProductRepository(context);
            CategoryRepository categories = new CategoryRepository(context);
            OrderRepository orders = new OrderRepository(context);
            OrderItemRepository orderItems = new OrderItemRepository(context);
            ShippingInfoRepository shippingInfoSet = new ShippingInfoRepository(context);
            return new UnitOfWork(context,products,categories,orders,orderItems,shippingInfoSet);
        }
        private void Seed()
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();

                        // seeding initial data data
                        var categoryOne = new Category();
                        categoryOne.Name = "Summer";
                        categoryOne.Slug = "summer";

                        var categoryTwo = new Category();
                        categoryTwo.Name = "Winter";
                        categoryTwo.Slug = "winter";

                        context.AddRange(categoryOne,categoryTwo);

                        var productOne = new Product(){
                            Name = "One",
                            Slug = "ProductOne",
                            Description = "Description",
                            Price = 1.99m,
                            Image = "",
                            Thumbnail = "",
                            DateAdded = DateTime.UtcNow
                        };

                        var productTwo = new Product(){
                            Name = "Two",
                            Slug = "ProductTwo",
                            Description = "Description2",
                            Price = 2.99m,
                            Image = "",
                            Thumbnail = "",
                            DateAdded = DateTime.UtcNow
                        };
                        var productThree = new Product(){
                            Name = "Three",
                            Slug = "ProductThree",
                            Description = "Description3",
                            Price = 5.99m,
                            Image = "",
                            Thumbnail = "",
                            DateAdded = DateTime.UtcNow
                        };
                        productOne.Category = categoryOne;
                        productTwo.Category = categoryOne;
                        productThree.Category = categoryTwo;

                        context.AddRange(productOne,productTwo,productThree);
                        
                        var user = new IdentityUser(){
                            Id = "1",
                            Email = "mail@mail.com",
                            UserName = "mail@mail.com",
                            PasswordHash = "AnyHash"
                        };
                        var user2 = new IdentityUser(){
                            Id= "2",
                            Email = "mail2@mail.com",
                            UserName = "mail2@mail.com",
                            PasswordHash = "AnyHash"
                        };

                        context.Users.AddRange(user,user2);
                        
                        
                        var order = new Order();
                        order.FirstName = "order1F";
                        order.LastName = "order1L";
                        order.Email = "mail@mail.com";
                        order.Address = "SomeStreet";
                        order.Place = "SomePlace";
                        order.Phone = "0115522";
                        order.UserId = user.Id;
                        order.PaidAmount = 10m;
                        order.ZipCode = "1191";
                        order.StripeToken = "SomeToken1";

                        var order2 = new Order();
                        order2.FirstName = "order1F";
                        order2.LastName = "order1L";
                        order2.Email = "mail@mail.com";
                        order2.Address = "SomeStreet2";
                        order2.Place = "SomePlace2";
                        order2.Phone = "0115523";
                        order2.UserId = user.Id;
                        order2.PaidAmount = 5m;
                        order2.ZipCode = "1192";
                        order2.StripeToken = "SomeToken2";

                        context.AddRange(order,order2);

                        var orderItem1 = new OrderItem();
                        orderItem1.Product = productOne;
                        orderItem1.Quantity = 3;
                        orderItem1.Price = orderItem1.Product.Price * orderItem1.Quantity;
                        orderItem1.Order = order;

                        var orderItem2 = new OrderItem();
                        orderItem2.Product = productTwo;
                        orderItem2.Quantity = 3;
                        orderItem2.Price = orderItem2.Product.Price * orderItem2.Quantity;
                        orderItem2.Order = order;
                        
                        var orderItem3 = new OrderItem();
                        orderItem3.Product = productThree;
                        orderItem3.Quantity = 6;
                        orderItem3.Price = orderItem3.Product.Price * orderItem3.Quantity;
                        orderItem3.Order = order2;

                        context.OrderItems.AddRange(orderItem1,orderItem2,orderItem3);
                        context.SaveChanges();


                        var shippingInfo = new ShippingInfo()
                        {
                            ShippedDate = DateTime.UtcNow
                        };
                        shippingInfo.OrderId = order.Id;

                        context.ShippingInfoSet.Add(shippingInfo);
                        context.SaveChanges();

                    }
                }
                _databaseInitialized = true;
            }
        }

        public void Dispose() => Connection.Dispose();
    }
}