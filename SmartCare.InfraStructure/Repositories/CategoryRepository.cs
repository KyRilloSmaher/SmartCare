using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public CategoryRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        #endregion

        #region Methods

        public async Task<IEnumerable<Category>> GetAllActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public IQueryable<Category> GetCategoriesQueryable(bool includeDeleted = false)
        {
            var query = _context.Categories.AsQueryable();

            if (!includeDeleted)
                query = query.Where(c => !c.IsDeleted);

            return query.AsNoTracking();
        }

        public override Task DeleteAsync(Category entity)
        {
            entity.IsDeleted = true;
            return UpdateAsync(entity);
        }

        public async Task<IEnumerable<Category>> SearchCategoryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return await GetAllActiveCategoriesAsync();

            var searchTerm = name.Trim().ToLower();
            var categoryList = await _context.Categories
                .Where(c => !c.IsDeleted && c.Name.ToLower().Contains(searchTerm))
                .AsNoTracking()
                .ToListAsync();

            return categoryList
                .Select(c => new
                {
                    Category = c,
                    Score = Fuzz.Ratio(c.Name.ToLower(), searchTerm)
                })
                .Where(x => x.Score >= 70)
                .Select(x => x.Category)
                .ToList();
        }

        #endregion
    }
}