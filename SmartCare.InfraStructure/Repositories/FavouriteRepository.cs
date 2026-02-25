using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class FavouriteRepository : GenericRepository<Favorite>, IFavouriteRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public FavouriteRepository(ApplicationDBContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
        #endregion

        #region Methods

        public async Task<IEnumerable<ProductProjectionDTO>> GetFavouritesByUserIdAsync(string userId)
        {
            return await _context.Favorites
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
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsProductFavoritedByUserAsync(string userId, Guid productId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.ClientId == userId && f.ProductId == productId);
        }

        public async Task<Favorite?> GetFavoriteAsync(string userId, Guid productId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.ClientId == userId);
        }

        public async Task<Favorite?> CheackFavouriteExistsAsync(string userId, Guid productId)
        {
            return await _context.Favorites.FirstOrDefaultAsync(f=>f.ProductId == productId && f.ClientId ==userId);
        }

        #endregion
    }
}