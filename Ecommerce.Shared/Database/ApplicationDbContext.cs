using Ecommerce.Shared.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Shared.Database
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShippingInfo> ShippingInfoSet { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
            .HasIndex(p => p.Name)
            .IsUnique();
            
            builder.Entity<Product>()
            .HasIndex(p => p.Slug)
            .IsUnique();

            builder.Entity<Category>()
            .HasIndex(p => p.Name)
            .IsUnique();

            builder.Entity<Category>()
            .HasIndex(p => p.Slug)
            .IsUnique();

            builder.Entity<Product>()
            .Property(p=>p.Price)
            .HasPrecision(8,2);

            builder.Entity<Order>()
            .Property(o=>o.PaidAmount)
            .HasPrecision(8,2);
            
            builder.Entity<OrderItem>()
            .Property(oi=>oi.Price)
            .HasPrecision(8,2);

            builder.Entity<Order>()
            .HasOne(x=>x.ShippingInfo)
            .WithOne(x=>x.Order)
            .HasForeignKey<ShippingInfo>(x=>x.OrderId);
        }
    }
}