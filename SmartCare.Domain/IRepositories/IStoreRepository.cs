using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IStoreRepository : IGenericRepository<Store>
    {
         Task<IEnumerable<Store>> GetAllStoresAsync();

         IQueryable<Store> GetStoresQueryable(bool includeDeleted = false);

          Task<Store?> GetStoreByIdAsync(Guid storeId, bool trackChanges = false);

          Task<IEnumerable<Store>> SearchStoresAsync(string searchTerm);
    }
}
