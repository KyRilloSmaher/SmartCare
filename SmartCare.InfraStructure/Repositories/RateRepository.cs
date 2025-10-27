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
            _context.Rates.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async override Task<Rate?> GetByIdAsync(Guid id, bool asTracking = false)
        {
            var entity = await _dbContext.Rates.FirstOrDefaultAsync(r=>r.Id ==id);
            if (entity == null) return null;

            if (!asTracking)
                _dbContext.Entry(entity).State = EntityState.Detached;

            return entity;
        }
        public async Task<IEnumerable<Rate>> GetRatesByProductIdAsync(Guid productId)
        {
            var rates = await _context.Rates
                                      .Where(r => r.ProductId == productId && !r.IsDeleted)
                                      .ToListAsync();
            return rates;
        }

        public async Task<IEnumerable<Rate>> GetRatesByUserIdAsync(string userId)
        {
           var rates = await _context.Rates
                                      .Where(r => r.ClientId == userId && !r.IsDeleted)
                                      .Include(r => r.Product)
                                          .ThenInclude(p => p.Images)
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
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.AverageRating = averageRate;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            return averageRate;  
        }
        #endregion
    }
}
