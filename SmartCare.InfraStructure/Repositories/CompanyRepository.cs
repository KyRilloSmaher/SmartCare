using FuzzySharp;
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
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        #region Feilds
        public readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public CompanyRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }


        public async Task<IEnumerable<Company>> GetAllCompaniesForAdminAsync()
        {
            return await base.GetAllAsync();
        }
        public async override Task<bool> DeleteAsync(Company entity) {
            entity.IsDeleted = true;
            _context.Companies.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Company>> SearchCompaniesByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return await GetAllCompaniesAsync();
            }
            var searchTerm = name.Trim().ToLower();
            var companiesList = await _context.Companies
                .Where(c => !c.IsDeleted && c.Name.Contains(name))
                .ToListAsync();
            var matchedCompanies = companiesList
                                        .Select(c => new
                                        {
                                            Category = c,
                                            Score = Fuzz.Ratio(c.Name.ToLower(), searchTerm)
                                        })
                                         .Where(x => x.Score >= 70)
                                         .Select(x => x.Category)
                                         .ToList();
            return matchedCompanies;
        }
        #endregion


    }
}
