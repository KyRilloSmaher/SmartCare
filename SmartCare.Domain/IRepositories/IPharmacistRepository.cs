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
        Task<bool> IsPharmacistPhoneNumberUniqueAsync(string phone);
        Task<Pharmacist> GetByLicenseNumberAsync(string licenseNumber);
        Task<IEnumerable<Pharmacist>> GetByBranchIdAsync(Guid branchId);
        Task RollbackTransactionAsync();
    }
}
