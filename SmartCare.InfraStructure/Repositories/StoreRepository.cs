using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class StoreRepository : GenericRepository<Store>,IStoreRepository
    {
        #region Feilds 
        private readonly ApplicationDBContext _context;
        #endregion
        #region Constructor
        public StoreRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        #endregion

        #region Methods

        public async Task<IEnumerable<Store>> GetAllStoresAsync()
        {
            return await _context.Stores
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }


        public async Task<IEnumerable<Store>> GetAllStoresForAdminAsync()
        {
            return await base.GetAllAsync();
        }


    public async Task<IEnumerable<Store>> SearchStoresAsync(string searchTerm)
    {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Store>();

            searchTerm = searchTerm.Trim().ToLower();


            var storesList = await _context.Stores
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var matchedStores = storesList
                .Select(s => new
                {
                    Store = s,
                    Score = Math.Max(
                        Fuzz.Ratio(s.Name.ToLower(), searchTerm),
                        Fuzz.Ratio(s.Address?.ToLower() ?? "", searchTerm)
                    )
                })
                .Where(x => x.Score >= 70 || x.Store.Name.Contains(searchTerm) || x.Store.Address.Contains(searchTerm))
                .Select(x => x.Store)
                .ToList();


            return matchedStores;
        }

    #endregion

}
}
