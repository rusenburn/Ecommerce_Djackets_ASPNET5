using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Shared.Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T>, IDisposable where T : class
    {
        protected readonly DbContext _context;
        private bool disposed = false;
        public RepositoryBase(DbContext context)
        {
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;
        }

        public async Task<T> GetOneAsync(int id)
        {
            if (id < 0) throw new ArgumentException($"{nameof(id)} cannot have a negative value.", nameof(id));
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<T> CreateOneAsync(T entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            var result = await _context.Set<T>().AddAsync(entity);
            return result.Entity;
        }

        public async Task CreateManyAsync(IEnumerable<T> entities)
        {
            if (entities is null) throw new ArgumentNullException(nameof(entities));
            var db = _context.Set<T>();
            await db.AddRangeAsync(entities);
        }

        public async Task<T> DeleteOneAsync(int id)
        {
            if (id <= 0) throw new ArgumentException($"{nameof(id)} cannot be less than 1", paramName: nameof(id));
            T entityToDelete = await GetOneAsync(id);
            if (entityToDelete is null) throw new InvalidOperationException("Entity Could not be found");
            var result = _context.Set<T>().Remove(entityToDelete);
            return result.Entity;
        }

        public async Task DeleteManyAsync(IEnumerable<T> entities)
        {
            if (entities is null) throw new ArgumentNullException(nameof(entities));
            if (entities.Any(x => x is null)) throw new ArgumentException($"{nameof(entities)} should contain a null element", paramName: nameof(entities));
            DbSet<T> dbSet = await Task.FromResult(_context.Set<T>());
            dbSet.RemoveRange(entities);
        }

        public async Task<T> UpdateOneAsync<Dto>(int id, Dto newEntity)
        {
            if (newEntity is null) throw new ArgumentNullException(nameof(newEntity));
            if (id <= 0) throw new ArgumentException($"{nameof(id)} cannot be less than 1.");
            T entityInDb = await GetOneAsync(id);
            if (entityInDb is null) throw new InvalidOperationException($"Entity with id of {id} Could not be found");
            _context.Entry(entityInDb).CurrentValues.SetValues(newEntity);
            _context.Entry(entityInDb).State = EntityState.Modified;
            return entityInDb;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // _context.Dispose();
                    this.disposed = true;
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}