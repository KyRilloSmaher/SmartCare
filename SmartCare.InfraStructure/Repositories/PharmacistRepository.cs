using Microsoft.AspNetCore.Identity;
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
    public class PharmacistRepository : GenericRepository<Pharmacist>, IPharmacistRepository
    {
        #region Feild
        private readonly ApplicationDBContext _context;
        #endregion

        public PharmacistRepository(ApplicationDBContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public async Task<IEnumerable<Pharmacist>> GetByBranchIdAsync(Guid branchId)
        {
            return await _context.Pharmacists.Where(p => p.StoreId == branchId).ToListAsync();
        }


        public async Task<Pharmacist> GetByLicenseNumberAsync(string licenseNumber)
        {
            return await _context.Pharmacists.FirstOrDefaultAsync(p => p.LicenseNumber == licenseNumber);
        }

        public async Task<Pharmacist?> GetByUserIdAsync(string userId, bool isTracked = false)
        {
            var query = _context.Pharmacists
                .Include(p => p.User)
                .Include(p => p.Store)
                .Where(p => p.Id == userId);

            query = isTracked
                ? query.AsTracking()
                : query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }


        public async Task<bool> IsPharmacistPhoneNumberUniqueAsync(string phone)
        {
            return !await _context.Users.AnyAsync(u => u.PhoneNumber == phone);
        }

        public async Task<bool> IsPharmacistLicenseNumberUniqueAsync(string licenseNumber)
        {
            return !await _context.Pharmacists
                .AnyAsync(p => p.LicenseNumber == licenseNumber);
        }
        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        // get unConfirmed pharmacists
        public async Task<IEnumerable<Pharmacist>> GetUnconfirmedPharmacistsAsync(bool asTracking = false)
        {
            var query = _context.Pharmacists
                                         .Include(p => p.User)
                                         .Include(p => p.Store)
                                         .Where(p => !p.User.EmailConfirmed);
            query = asTracking ? query.AsTracking() : query.AsNoTracking();
            return await query.ToListAsync();
        }
        public virtual async Task<IEnumerable<Pharmacist>> GetAllAsync(bool asTracking = false)
        {
            IQueryable<Pharmacist> query = _context.Pharmacists
                .Include(p => p.User)
                .Include(p => p.Store);

            if (!asTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }
    }
}
