using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface ICompanyRepository :IGenericRepository<Company>
    {
        Task<IEnumerable<Company>> GetAllCompaniesAsync();
        Task<IEnumerable<Company>> GetAllCompaniesForAdminAsync();
        Task<IEnumerable<Company>> SearchCompaniesByNameAsync(string name);
    }
}
