using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
        public interface IGenericRepository<T> where T : class
        {
            #region CRUD Operations

            Task<T> AddAsync(T entity);
            Task<T> Add(T entity);
            Task AddRangeAsync(ICollection<T> entities);

            Task<T> UpdateAsync(T entity);
            Task UpdateRangeAsync(ICollection<T> entities);

            Task<bool> DeleteAsync(T entity);
            Task DeleteRangeAsync(ICollection<T> entities);

            Task<IEnumerable<T>> GetAllAsync(bool asTracking = false);
            Task<T?> GetByIdAsync(Guid id, bool asTracking = false);

            Task<IQueryable<T>> FilterListAsync<TKey>(
                Expression<Func<T, TKey>> orderBy,
                Expression<Func<T, bool>>? searchPredicate = null,
                bool ascending = true);

            Task SaveChangesAsync();

            #endregion

            #region Transaction Handling

            /// <summary>
            /// Begins a new database transaction.
            /// If a transaction is already active, this call will be ignored.
            /// </summary>
            Task BeginTransactionAsync();

            /// <summary>
            /// Commits the current transaction and saves all pending changes.
            /// </summary>
            Task CommitTransactionAsync();

            /// <summary>
            /// Rolls back the current transaction and disposes it.
            /// </summary>
            Task RollBackAsync();

            #endregion
        }
    

}
