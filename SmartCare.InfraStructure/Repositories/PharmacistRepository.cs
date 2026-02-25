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
        private readonly UserManager<Pharmacist> _PharmacistManager;
        private readonly ApplicationDBContext _context;

        #endregion

        public PharmacistRepository(UserManager<Pharmacist> pharmacistManager, ApplicationDBContext context) : base(context)
        {
            _PharmacistManager = pharmacistManager;
            _context = context;
        }

        public async Task<bool> AddToRoleAsync(Pharmacist pharmacist, string role)
        {
            var result = await _PharmacistManager.AddToRoleAsync(pharmacist, role);
            return result.Succeeded;
        }

        public async Task<IdentityResult> CreatepharmacistAsync(Pharmacist pharmacist, string password)
        {
            var result = await _PharmacistManager.CreateAsync(pharmacist, password);
            return result;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(Pharmacist pharmacist)
        {
            return await _PharmacistManager.GenerateEmailConfirmationTokenAsync(pharmacist);
        
        }

        public async Task<IEnumerable<Pharmacist>> GetByBranchIdAsync(Guid branchId)
        {
           return await _context.pharmacists.Where(p => p.StoreId == branchId).ToListAsync();
        }

        public async Task<Pharmacist> GetByEmailAsync(string email)
        {
            return await _PharmacistManager.FindByEmailAsync(email);
        }

        public async Task<Pharmacist> GetByLicenseNumberAsync(string licenseNumber)
        {
            return await _context.pharmacists.FirstOrDefaultAsync(p => p.LicenseNumber == licenseNumber);
        }

        public async Task<bool> IspharmacistPhoneNumberUniqueAsync(string phone)
        {
            bool exists = await _PharmacistManager.Users.AnyAsync(u => u.PhoneNumber == phone);
            return !exists;
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task<Pharmacist> SearchByNameAsync(string name)
        {
            return await _context.Set<Pharmacist>()
        .FirstOrDefaultAsync(p => p.FirstName.Contains(name));
        }
    }
}
