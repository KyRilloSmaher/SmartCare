using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class RateRepository : GenericRepository<Rate>,IRateRepository
    {
        #region Feilds
        private readonly  ApplicationDBContext _context;
        #endregion
        #region Constructors
        public RateRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        #endregion
        #region Custom Methods
        public async override Task<bool> DeleteAsync(Rate entity)
        {
            entity.IsDeleted = true;
            var product = await _context.Products.FindAsync(entity.ProductId);
            if (product != null) {
                if (product.TotalRatings > 0)
                    product.TotalRatings -= 1;
            }
            _context.Rates.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async override Task<Rate?> GetByIdAsync(Guid id, bool asTracking = false)
        {
            var entity = await _dbContext.Rates.Include(r => r.Product)
                                                    .ThenInclude(p => p.Images)
                                               .FirstOrDefaultAsync(r=>r.Id ==id);
            if (entity == null) return null;

            if (!asTracking)
                _dbContext.Entry(entity).State = EntityState.Detached;

            return entity;
        }
        public async Task<IEnumerable<Rate>> GetRatesByProductIdAsync(Guid productId)
        {
            var rates = await _context.Rates
                                      .Include(r => r.Product)
                                          .ThenInclude(p => p.Images)
                                      .Where(r => r.ProductId == productId && !r.IsDeleted).AsNoTracking()
                                      .ToListAsync();
            return rates;
        }

        public async Task<IEnumerable<Rate>> GetRatesByUserIdAsync(string userId)
        {
           var rates = await _context.Rates
                                      .Include(r => r.Product)
                                          .ThenInclude(p => p.Images)
                                      .Where(r => r.ClientId == userId && !r.IsDeleted)
                                      .AsNoTracking()
                                      .ToListAsync();
            return  rates;
        }

        public async Task<float> UpdateAverageRateForProductAsync(Guid productId)
        {
            var rates = await GetRatesByProductIdAsync(productId);
            if (rates == null || !rates.Any())
            {
                return 0;
            }
            float averageRate = (float)rates.Average(r => r.Value);
            int ratesCount = rates.Where(r=>r.ProductId == productId).Count();
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.AverageRating = averageRate;
                product.TotalRatings = ratesCount;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            return averageRate;  
        }
        public async Task<bool> IsProductRatedByUserAsync(string userId, Guid productId)
        {
            return await _context.Rates.AnyAsync(r => r.ClientId == userId && r.ProductId == productId && !r.IsDeleted);
        }

        public async Task<bool> MarkAllClientRatesAsDeleted(string userId)
        {
            var rates = await _context.Rates
                                      .Where(r => r.ClientId == userId && !r.IsDeleted)
                                      .ToListAsync();
            if (rates == null)
            {
                return false;
            }
            foreach (var rate in rates)
            {
                await UpdateAverageRateForProductAsync(rate.ProductId);
                rate.IsDeleted = true;
            }
            _context.Rates.UpdateRange(rates);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
