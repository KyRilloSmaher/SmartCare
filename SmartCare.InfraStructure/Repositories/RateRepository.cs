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
                                      .ToListAsync();
            return  rates;
        }
        #endregion
    }
}
