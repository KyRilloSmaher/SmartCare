using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Projection_Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IFavouriteRepository : IGenericRepository<Favorite>
    {
      Task<IEnumerable<ProductProjectionDTO>> GetFavouritesByUserIdAsync(string userId);

      Task<bool> IsProductFavoritedByUserAsync(string userId, Guid productId);
      Task<Favorite?> GetFavoriteAsync(string userId, Guid productId);
      Task<Favorite?> CheackFavouriteExistsAsync(string userId, Guid productId);
    }
}
