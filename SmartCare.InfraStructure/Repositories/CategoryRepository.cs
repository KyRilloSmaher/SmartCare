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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        #region Feilds
        public readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public CategoryRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }


        public async Task<IEnumerable<Category>> GetAllCategoriesForAdminAsync()
        {
            return await base.GetAllAsync();
        }
        public async override Task<bool> DeleteAsync(Category entity)
        {
            entity.IsDeleted = true;
            _context.Categories.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Category>> SearchCategoryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return await GetAllCategoriesAsync();
            }
            var searchTerm = name.Trim().ToLower();
            var catgoryList =await _context.Categories
                .Where(c => !c.IsDeleted && c.Name.Contains(name))
                .ToListAsync();
            var matchedCategories = catgoryList
                                        .Select(c => new
                                        {
                                            Category = c,
                                            Score = Fuzz.Ratio(c.Name.ToLower(), searchTerm)
                                        })
                                         .Where(x => x.Score >= 70)
                                         .Select(x => x.Category)
                                         .ToList();
            return matchedCategories;
        }
        #endregion
    }
}
