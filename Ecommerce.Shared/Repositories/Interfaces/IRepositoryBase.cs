using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ecommerce.Shared.Repositories.Interfaces
{
    public interface IRepositoryBase<T> : IDisposable where T : class
    {
        Task<T> GetOneAsync(int id);

        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> FindAsync(Expression<Func<T,bool>> predicate);

        
        Task CreateManyAsync(IEnumerable<T> entities);

        Task<T> CreateOneAsync(T entity);
    
        Task<T> DeleteOneAsync(int id);

        Task DeleteManyAsync(IEnumerable<T> entities);

        Task<T> UpdateOneAsync<Dto>(int id,Dto entity);
    }
}