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
        public  Task<IEnumerable<Store>> GetAllStoresAsync();

        public  Task<IEnumerable<Store>> GetAllStoresForAdminAsync();

        public Task<IEnumerable<Store>> SearchStoresAsync(string searchTerm);
    }
}
