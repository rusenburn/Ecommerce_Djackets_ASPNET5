using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;

namespace Ecommerce.Shared.Repositories.Interfaces
{
    public interface IProductRepository : IRepositoryBase<Product>
    {
        Task<IEnumerable<Product>> GetLatestProducts(int count);
        Task<Product> GetProductBySlugs(string categorySlug , string productSlug);
        Task<IEnumerable<Product>> GetProductsWithCategoriesAsync(int pageIndex=1, int pageSize=int.MaxValue,string query="");
    }
}