using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class StoreRepository : GenericRepository<Store>, IStoreRepository
    {
        #region Fields 
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
                .Where(s => !s.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public IQueryable<Store> GetStoresQueryable(bool includeDeleted = false)
        {
            var query = _context.Stores.AsQueryable();

            if (!includeDeleted)
                query = query.Where(s => !s.IsDeleted);

            return query.AsNoTracking();
        }

        public async Task<Store?> GetStoreByIdAsync(Guid storeId, bool trackChanges = false)
        {
            var query = _context.Stores.Where(s => s.Id == storeId);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Store>> SearchStoresAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Store>();

            searchTerm = searchTerm.Trim().ToLower();

            var storesList = await _context.Stores
                .Where(s => !s.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            return storesList
                .Select(s => new
                {
                    Store = s,
                    Score = Math.Max(
                        Fuzz.Ratio(s.Name.ToLower(), searchTerm),
                        Fuzz.Ratio(s.Address?.ToLower() ?? "", searchTerm)
                    )
                })
                .Where(x => x.Score >= 70 ||
                           x.Store.Name.ToLower().Contains(searchTerm) ||
                           (x.Store.Address != null && x.Store.Address.ToLower().Contains(searchTerm)))
                .Select(x => x.Store)
                .ToList();
        }

        #endregion
    }
}