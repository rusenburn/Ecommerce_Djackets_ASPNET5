using System;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Shared.Repositories
{
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        protected ApplicationDbContext Context { get => _context as ApplicationDbContext; }
        public CategoryRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public async Task<Category> GetCategoryBySlug(string categorySlug)
        {
            if(string.IsNullOrEmpty(categorySlug)) throw new ArgumentNullException(nameof(categorySlug));
            return await Context.Categories.FirstOrDefaultAsync(c=>c.Slug == categorySlug);
        }
    }
}