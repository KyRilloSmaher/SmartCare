using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IRateRepository : IGenericRepository<Rate>
    {
        Task<IEnumerable<Rate>> GetRatesByUserIdAsync(string userId);
        Task<IEnumerable<Rate>> GetRatesByProductIdAsync(Guid productId);

    }
}
