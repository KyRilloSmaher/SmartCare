using Microsoft.AspNetCore.Identity;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IPharmacistRepository : IGenericRepository<Pharmacist>
    {
        Task<Pharmacist> GetByEmailAsync(string email);
        Task<Pharmacist> SearchByNameAsync(string name);
        Task<Pharmacist> GetByLicenseNumberAsync(string licenseNumber);
        Task<IEnumerable<Pharmacist>> GetByBranchIdAsync(Guid branchId);
        Task<string> GenerateEmailConfirmationTokenAsync(Pharmacist pharmacist);
        Task RollbackTransactionAsync();
        Task<bool> AddToRoleAsync(Pharmacist pharmacist, string role);
        Task<IdentityResult> CreatepharmacistAsync(Pharmacist pharmacist, string password);
        Task<bool> IspharmacistPhoneNumberUniqueAsync(string phone);
    }
}
