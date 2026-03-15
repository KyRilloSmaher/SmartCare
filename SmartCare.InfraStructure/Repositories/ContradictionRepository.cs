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
            return await _context.Contradictions
                .FirstOrDefaultAsync(c =>
                    (c.Ingredient_A == ingredientA && c.Ingredient_B == ingredientB) ||
                    (c.Ingredient_A == ingredientB && c.Ingredient_B == ingredientA)
                );
        }
    }
}
