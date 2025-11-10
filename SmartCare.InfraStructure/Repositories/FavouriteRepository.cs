using Microsoft.EntityFrameworkCore;
//using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class FavouriteRepository : GenericRepository<Favorite>, IFavouriteRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructors
        public FavouriteRepository(ApplicationDBContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        #endregion

        #region Custom Methods

        public async Task<IEnumerable<ProductProjectionDTO>> GetFavouritesByUserIdAsync(string userId)
        {
            var Favourites = await _context.Favorites
                                            .Include(f => f.Product)
                                             .ThenInclude(p => p.Images)
                                            .Where(f => f.ClientId == userId && !f.Product.IsDeleted)
                                            .Select(f => new ProductProjectionDTO
                                            {
                                                ProductId = f.Product.ProductId,
                                                ProductNameAr = f.Product.NameAr,
                                                ProductNameEn = f.Product.NameEn,
                                                Description = f.Product.Description,
                                                MainImageUrl = f.Product.Images.FirstOrDefault().Url,
                                                 TotalRatings = f.Product.TotalRatings,
                                                Price = f.Product.Price,
                                                IsAvailable = f.Product.IsAvailable
                                            }).ToListAsync();


            return Favourites;
        }

        public async Task<bool> IsProductFavoritedByUserAsync(string userId, Guid productId)
        {
            return await _context.Favorites.AnyAsync(f => f.ClientId == userId && f.ProductId == productId);
        }

        public async Task<Favorite?> checkFavoriteExists(string userId , Guid productId)
        {
            return await _context.Favorites.FirstOrDefaultAsync(r => r.ProductId == productId && r.ClientId == userId);
        }
        #endregion

    }
}
