using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System.Linq.Expressions;

namespace SmartCare.InfraStructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        #region Fields
        protected readonly ApplicationDBContext _context;
        protected readonly DbSet<T> _dbSet;
        #endregion

        #region Constructor
        public GenericRepository(ApplicationDBContext dbContext)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _context.Set<T>();
        }
        #endregion

        #region CRUD Operations

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<ICollection<T>> AddRangeAsync(ICollection<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(ICollection<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task UpdateRangeAsync(ICollection<T> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(bool asTracking = false)
        {
            return asTracking
                ? await _dbSet.ToListAsync()
                : await _dbSet.AsNoTracking().ToListAsync();
        }

        public virtual IQueryable<T> GetQueryable(bool asTracking = false)
        {
            return asTracking ? _dbSet.AsQueryable() : _dbSet.AsNoTracking();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, bool asTracking = false)
        {
            var entity = await _dbSet.FindAsync(id);

            if (entity != null && !asTracking)
                _context.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        public virtual async Task<IQueryable<T>> FilterListAsync<TKey>(
            Expression<Func<T, TKey>> orderBy,
            Expression<Func<T, bool>>? searchPredicate = null,
            bool ascending = true)
        {
            IQueryable<T> query = _dbSet;

            if (searchPredicate is not null)
                query = query.Where(searchPredicate);

            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return await Task.FromResult(query);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);
        }

        #endregion
    }
}