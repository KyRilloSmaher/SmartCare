using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.Domain.IRepositories;


namespace SmartCare.InfraStructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        #region  Feild(s)
        protected readonly ApplicationDBContext _dbContext;
        #endregion

        #region Constructor(s)

        public GenericRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext ?? throw new ArgumentNullException(nameof(_dbContext), "ApplicationDbContext cannot be null.");

        }
        #endregion


        #region Handle Methods 
        public async Task DeleteRangeAsync(ICollection<T> entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Deleted;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async virtual Task<IEnumerable<T>> GetAllAsync(bool AsTracking = false)
        {
            return AsTracking
                          ? await _dbContext.Set<T>().ToListAsync()
                          : await _dbContext.Set<T>().AsNoTracking().ToListAsync();

        }

        public async virtual Task<T?> GetByIdAsync(Guid id, bool AsTracking = false)
        {
            var entity = await _dbContext.Set<T>().FindAsync(id);
            if (entity == null) return null;

            if (!AsTracking)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
            }

            return entity;
        }


        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _dbContext.Database.BeginTransaction();
        }

        public void Commit()
        {
            _dbContext.Database.CommitTransaction();
        }

        public void RollBack()
        {
            _dbContext.Database.RollbackTransaction();
        }


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

        public async virtual Task<bool>  DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _dbContext.Database.CommitTransactionAsync();
        }

        public async Task RollBackAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }

        public virtual async Task<IQueryable<T>> FilterListAsync<TKey>(
            Expression<Func<T, TKey>> orderBy,
            Expression<Func<T, bool>> searchPredicate = null,
            bool ascendenig = true)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (searchPredicate is not null)
            {
                query = query.Where(searchPredicate);
            }
            query = ascendenig ?
                                query.OrderBy(orderBy)
                                : query.OrderByDescending(orderBy);
            return await Task.FromResult(query);
        }



        #endregion
    }
}
