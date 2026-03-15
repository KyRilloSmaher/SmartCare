using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IContradictionRepository : IGenericRepository<Contradiction>
    {
       Task<Contradiction?> ContradictionExistsAsync(string ingredientA, string ingredientB);
    }
}
