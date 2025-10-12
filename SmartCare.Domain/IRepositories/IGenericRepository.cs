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
        Task<IEnumerable<T>> GetAllAsync(bool AsTracking = false);
        Task<T?> GetByIdAsync(Guid id, bool AsTracking = false);
        Task<T> AddAsync(T entity);
        Task<T> Add(T entity);
        Task<T> UpdateAsync(T entity);
        Task UpdateRangeAsync(ICollection<T> entities);
        Task<bool> DeleteAsync(T entity);
        Task<IQueryable<T>> FilterListAsync<TKey>(Expression<Func<T, TKey>> orderBy, Expression<Func<T, bool>> searchPredicate = null, bool ascendenig = true);
        Task AddRangeAsync(ICollection<T> entities);
        Task DeleteRangeAsync(ICollection<T> entities);

        Task SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
        void Commit();
        void RollBack();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync();
        Task RollBackAsync();
    }
}
