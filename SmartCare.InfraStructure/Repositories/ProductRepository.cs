using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
using SmartCare.InfraStructure.DbContexts;
using System.Linq;
using System.Linq.Expressions;

namespace SmartCare.InfraStructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        #region Fields
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructor
        public ProductRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        #endregion

        #region Query Methods

        public override async Task<Product?> GetByIdAsync(Guid id, bool asTracking = false)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Company)
                .Where(p => p.ProductId == id);

            return asTracking
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task<List<Product>> FilterListAsync(
    Expression<Func<Product, bool>>? searchPredicate = null)
        {
            IQueryable<Product> query = _context.Products;

            if (searchPredicate != null)
                query = query.Where(searchPredicate);

            return await query.ToListAsync();
        }
        public IQueryable<Product> GetAllProductsQuerable(bool includeDeleted = false)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Company)
                .Include(p => p.Category)
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(p => !p.IsDeleted);

            return query.AsNoTracking();
        }
        public async Task<bool> CalculateProductAvailabilty(Guid productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product is null)
                throw new InvalidOperationException("Try To Change Availablity Of Non Existing product");

            var isAvailble = await _context.Inventories.AnyAsync(inv => inv.ProductId == productId && Math.Min(0, inv.StockQuantity - inv.ReservedQuantity) > 0);
            product.IsAvailable = isAvailble;
            return isAvailble;
        }
        public IQueryable<Product> FilterProductsAsync(FilterProductsDTo filter)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Company)
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted);

            if (filter.FromRate.HasValue)
                query = query.Where(p => p.AverageRating >= filter.FromRate.Value);

            if (filter.ToRate.HasValue)
                query = query.Where(p => p.AverageRating <= filter.ToRate.Value);

            if (filter.FromPrice.HasValue)
                query = query.Where(p => p.Price >= filter.FromPrice.Value);

            if (filter.ToPrice.HasValue)
                query = query.Where(p => p.Price <= filter.ToPrice.Value);

            // Order By
            if (filter.OrderByName.HasValue)
                query = filter.OrderByName.Value
                    ? query.OrderBy(f => f.NameEn)
                    : query.OrderByDescending(f => f.NameEn);
            else if (filter.OrderByPrice.HasValue)
                query = filter.OrderByPrice.Value
                    ? query.OrderBy(f => f.Price)
                    : query.OrderByDescending(f => f.Price);
            else if (filter.OrderByRate.HasValue)
                query = filter.OrderByRate.Value
                    ? query.OrderBy(f => f.AverageRating)
                    : query.OrderByDescending(f => f.AverageRating);

            return query.AsNoTracking();
        }

        public async Task<Product?> SearchProductByNameAsync(string nameEn)
        {
            if (string.IsNullOrWhiteSpace(nameEn))
                return null;

            var trimmedName = nameEn.Trim().ToLower();

            return await _context.Products
                .Include(p => p.Images)
                .Where(p => p.NameEn.ToLower().Contains(trimmedName))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public IQueryable<Product> SearchProductsByDescriptionAsync(string partialDescription)
        {
            if (string.IsNullOrWhiteSpace(partialDescription))
                return Enumerable.Empty<Product>().AsQueryable();

            string trimmedDescription = partialDescription.Trim().ToLower();

            return _context.Products
                .Include(p => p.Images)
                .Where(p => p.Description != null &&
                           p.Description.Trim().ToLower().Contains(trimmedDescription))
                .AsNoTracking();
        }

        public IQueryable<Product> SearchProductsByCompanyName(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return Enumerable.Empty<Product>().AsQueryable();

            string trimmedCompanyName = companyName.Trim().ToLower();

            return _context.Products
                .Include(p => p.Company)
                .Include(p => p.Images)
                .Where(p => p.Company.Name.Trim().ToLower().Contains(trimmedCompanyName))
                .AsNoTracking();
        }

        public IQueryable<Product> GetProductsByCompanyId(Guid companyId)
        {
            if (companyId == Guid.Empty)
                return Enumerable.Empty<Product>().AsQueryable();

            return _context.Products
                .Include(p => p.Images)
                .Where(p => p.CompanyId == companyId)
                .AsNoTracking();
        }

        public IQueryable<Product> SearchProductsByCategoryName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return Enumerable.Empty<Product>().AsQueryable();

            string trimmedCategoryName = categoryName.Trim().ToLower();

            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.Category != null &&
                           p.Category.Name.Trim().ToLower().Contains(trimmedCategoryName))
                .AsNoTracking();
        }

        public IQueryable<Product> GetProductsByCategoryId(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
                return Enumerable.Empty<Product>().AsQueryable();

            return _context.Products
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId)
                .AsNoTracking();
        }

        public IQueryable<Product> GetMostSelling()
        {
            return _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Join(
                    _context.Products.Include(p => p.Images),
                    sales => sales.ProductId,
                    product => product.ProductId,
                    (sales, product) => product
                )
                .AsNoTracking();
        }

        public IQueryable<Product> GetMorePopular()
        {
            return _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .Join(
                    _context.Products.Include(p => p.Images),
                    popularity => popularity.ProductId,
                    product => product.ProductId,
                    (popularity, product) => product
                )
                .AsNoTracking();
        }

        public override Task DeleteAsync(Product product)
        {
            product.IsDeleted = true;
            return UpdateAsync(product);
        }

       
        #endregion
    }
}