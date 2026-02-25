using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class RateRepository : GenericRepository<Rate>, IRateRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public RateRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        #endregion

        #region Query Methods

        public IQueryable<Rate> GetRatesQueryable(bool trackChanges = false)
        {
            var query = _context.Rates
                .Include(r => r.Product)
                    .ThenInclude(p => p.Images)
                .Where(r => !r.IsDeleted);

            return trackChanges ? query : query.AsNoTracking();
        }

        public override async Task<Rate?> GetByIdAsync(Guid id, bool asTracking = false)
        {
            var query = _context.Rates
                .Include(r => r.Product)
                    .ThenInclude(p => p.Images)
                .Where(r => r.Id == id && !r.IsDeleted);

            return asTracking
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Rate>> GetRatesByProductIdAsync(Guid productId)
        {
            return await _context.Rates
                .Include(r => r.Product)
                    .ThenInclude(p => p.Images)
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Rate>> GetRatesByUserIdAsync(string userId)
        {
            return await _context.Rates
                .Include(r => r.Product)
                    .ThenInclude(p => p.Images)
                .Where(r => r.ClientId == userId && !r.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsProductRatedByUserAsync(string userId, Guid productId)
        {
            return await _context.Rates
                .AnyAsync(r => r.ClientId == userId &&
                              r.ProductId == productId &&
                              !r.IsDeleted);
        }

        public async Task<float> GetAverageRatingForProductAsync(Guid productId)
        {
            var rates = await _context.Rates
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .Select(r => r.Value)
                .ToListAsync();

            return rates.Any() ? (float)rates.Average() : 0;
        }

        public async Task<int> GetRatingCountForProductAsync(Guid productId)
        {
            return await _context.Rates
                .CountAsync(r => r.ProductId == productId && !r.IsDeleted);
        }

        #endregion

        #region Business Logic Methods

        public override Task DeleteAsync(Rate entity)
        {
            entity.IsDeleted = true;
            return UpdateAsync(entity);
        }

        public async Task<float> UpdateAverageRateForProductAsync(Guid productId)
        {
            var rates = await GetRatesByProductIdAsync(productId);

            if (!rates.Any())
                return 0;

            float averageRate = (float)rates.Average(r => r.Value);
            int ratesCount = rates.Count();

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.AverageRating = averageRate;
                product.TotalRatings = ratesCount;
                // Product is tracked, no need for Update or SaveChanges here
            }

            return averageRate;
        }

        public async Task<bool> MarkAllClientRatesAsDeletedAsync(string userId)
        {
            var rates = await _context.Rates
                .Where(r => r.ClientId == userId && !r.IsDeleted)
                .ToListAsync();

            if (!rates.Any())
                return false;

            foreach (var rate in rates)
            {
                rate.IsDeleted = true;
                // Update product average rating will happen when SaveChanges is called
            }

            return true;
        }

        #endregion
    }
}