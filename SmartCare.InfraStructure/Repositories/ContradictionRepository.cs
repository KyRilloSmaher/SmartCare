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
    public class ContradictionRepository : GenericRepository<Contradiction>,IContradictionRepository
    {
        public ContradictionRepository(ApplicationDBContext context):base(context) { }

        public async Task<Contradiction?> ContradictionExistsAsync(string ingredientA, string ingredientB)
        {
            string a = ingredientA.Trim().ToLower();
            string b = ingredientB.Trim().ToLower();

            return await _context.Contradictions
                .FirstOrDefaultAsync(c =>
                    (c.Ingredient_A.ToLower().Contains(a) && c.Ingredient_B.ToLower().Contains(b)) ||
                    (c.Ingredient_A.ToLower().Contains(b) && c.Ingredient_B.ToLower().Contains(a))
                );
        }

        public async Task<List<Contradiction>> GetContradictionsForIngredientAsync(string ingredient)
        {
            return await _context.Contradictions
                .Where(c => c.Ingredient_A == ingredient || c.Ingredient_B == ingredient)
                .ToListAsync();
        }
    }
}
