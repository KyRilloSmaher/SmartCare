using SmartCare.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.Domain.IRepositories
{
    public interface IFavouriteRepository : IGenericRepository<Favorite>
    {
        Task<IEnumerable<ProductProjectionDTO>> GetFavouritesByUserIdAsync(string userId);
        Task<bool> IsProductFavoritedByUserAsync(string userId, Guid productId);
        Task<Favorite?> checkFavoriteExists(string userId, Guid productId);
    }
}
