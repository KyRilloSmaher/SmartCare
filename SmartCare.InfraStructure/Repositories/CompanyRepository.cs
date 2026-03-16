using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
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
                .AsNoTracking().OrderByDescending(c => c.ProductsCount)
                .ToListAsync();
        }

        public IQueryable<Company> GetAllCompaniesQuerable(bool includeDeleted = false)
        {
            var query = _context.Companies.AsQueryable();

            if (!includeDeleted)
                query = query.Where(c => !c.IsDeleted).OrderByDescending(c => c.ProductsCount);

            return query.AsNoTracking();
        }

        public override Task DeleteAsync(Company entity)
        {
            entity.IsDeleted = true;
            return UpdateAsync(entity);
        }

        public async Task<IEnumerable<Company>> SearchCompaniesByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return await GetAllCompaniesAsync();

            var searchTerm = name.Trim().ToLower();
            var companiesList = await _context.Companies
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            return companiesList
                .Select(c => new
                {
                    Company = c,
                    Score = Fuzz.Ratio(c.Name.ToLower(), searchTerm)
                })
                .Where(x => x.Score >= 70)
                .Select(x => x.Company)
                .ToList();
        }
        public async Task<IEnumerable<Company>> GetAllCompaniesForAdminAsync()
        {
            return await _context.Companies
                .AsNoTracking().OrderByDescending(c => c.ProductsCount)
                .ToListAsync();
        }

        #endregion
    }
}