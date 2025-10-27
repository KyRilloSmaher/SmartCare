using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System.Linq.Expressions;

namespace SmartCare.InfraStructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        #region Fields
        protected readonly ApplicationDBContext _dbContext;
        private IDbContextTransaction? _currentTransaction;
        #endregion

        #region Constructor
        public GenericRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        #endregion

        #region CRUD Operations

        public async virtual Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<T> Add(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(ICollection<T> entities)
        {
            await _dbContext.Set<T>().AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateRangeAsync(ICollection<T> entities)
        {
            _dbContext.Set<T>().UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async virtual Task<bool> DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task DeleteRangeAsync(ICollection<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async virtual Task<IEnumerable<T>> GetAllAsync(bool asTracking = false)
        {
            return asTracking
                ? await _dbContext.Set<T>().ToListAsync()
                : await _dbContext.Set<T>().AsNoTracking().ToListAsync();
        }

        public async virtual Task<T?> GetByIdAsync(Guid id, bool asTracking = false)
        {
            var entity = await _dbContext.Set<T>().FindAsync(id);
            if (entity == null) return null;

            if (!asTracking)
                _dbContext.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<IQueryable<T>> FilterListAsync<TKey>(
            Expression<Func<T, TKey>> orderBy,
            Expression<Func<T, bool>>? searchPredicate = null,
            bool ascending = true)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (searchPredicate is not null)
                query = query.Where(searchPredicate);

            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return await Task.FromResult(query);
        }

        #endregion

        #region Transaction Handling

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
                return; // Already in transaction

            _currentTransaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
                return;

            try
            {
                // Ensure all pending changes are saved before committing
                await _dbContext.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollBackAsync()
        {
            if (_currentTransaction == null)
                return;

            try
            {
                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        #endregion
    }
}
