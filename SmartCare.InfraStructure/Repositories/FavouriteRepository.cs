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

        public async Task<IEnumerable<FavoriteResponseDtoR>> GetFavouritesByUserIdAsync(string userId)
        {
            var Favourites = await _context.Favorites
                                .Where(f => f.ClientId == userId && !f.Product.IsDeleted && f.Product.IsAvailable)
                                .Select(f => new FavoriteResponseDtoR
                                   {
                                     ProductId = f.Product.ProductId,
                                     ProductNameAr = f.Product.NameAr,
                                     ProductNameEn = f.Product.NameEn,
                                     Description_Of_Product = f.Product.Description,
                                     TotalRatings = f.Product.TotalRatings,
                                     Price = f.Product.Price,
                                     IsAvailable = f.Product.IsAvailable
                                    })
                                   .ToListAsync();


            return Favourites;
        }
        #endregion

    }
}
