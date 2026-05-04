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
        Task<ICollection<T>> AddRangeAsync(ICollection<T> entities);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(ICollection<T> entities);
        Task UpdateAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync(bool asTracking = false);
        Task<T?> GetByIdAsync(Guid id, bool asTracking = false);

        Task<IQueryable<T>> FilterListAsync<TKey>(
            Expression<Func<T, TKey>> orderBy,
            Expression<Func<T, bool>>? searchPredicate = null,
            bool ascending = true);

        #endregion

    }


}
