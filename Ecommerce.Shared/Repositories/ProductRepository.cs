using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Shared.Repositories
{
    public class ProductRepository : RepositoryBase<Product>, IProductRepository
    {
        protected ApplicationDbContext Context { get => _context as ApplicationDbContext; }
        public ProductRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetLatestProducts(int count)
        {
            if (count < 0) throw new ArgumentNullException(nameof(count));
            if (count == 0) return new Product[0];
            return await Context.Products.OrderByDescending(p => p.DateAdded).Take(count).ToListAsync();
        }

        public async Task<Product> GetProductBySlugs(string categorySlug, string productSlug)
        {
            if (string.IsNullOrEmpty(categorySlug) ||
                string.IsNullOrEmpty(productSlug) ||
                string.IsNullOrWhiteSpace(categorySlug) ||
                string.IsNullOrWhiteSpace(productSlug)
                ) return null;
            return await Context.Products.Where(p => p.Category.Slug == categorySlug && p.Slug == productSlug).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsWithCategoriesAsync(int pageIndex = 1, int pageSize = int.MaxValue, string query = "")
        {
            if (pageIndex <= 0) throw new ArgumentException($"{nameof(pageIndex)} cannot have 0 or negative values", nameof(pageIndex));
            if (pageSize < 0) throw new ArgumentException($"{nameof(pageSize)} cannot have negative values", nameof(pageSize));
            query = query?.Trim() ?? "";

            return await Context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(query) || p.Slug.Contains(query) || p.Description.Contains(query))
                .OrderByDescending(p => p.DateAdded)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
   
    }
}