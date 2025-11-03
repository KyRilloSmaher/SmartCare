using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Projection_Models;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
//using static SmartCare.API.Helpers.ApplicationRouting;

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

        #region Methods
        public IQueryable<Product> FilterProductsAsync(FilterProductsDTo filterProductsDTo)
        {
            IQueryable<Product> query = _context.Products;

            if (!string.IsNullOrWhiteSpace(filterProductsDTo.companyName))
            {
                query =  query.Where(p => p.Company.Name.ToLower() == filterProductsDTo.companyName.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filterProductsDTo.categoryName))
            {
                query = query.Where(p => p.Category.Name.ToLower() == filterProductsDTo.categoryName.Trim().ToLower());
            }

            if (filterProductsDTo.FromRate.HasValue)
            {
                query = query.Where(p => p.AverageRating >= filterProductsDTo.FromRate.Value);
            }

            if (filterProductsDTo.ToRate.HasValue)
            {
                query = query.Where(p => p.AverageRating <= filterProductsDTo.ToRate.Value);
            }

            if (filterProductsDTo.FromPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filterProductsDTo.FromPrice.Value);
            }

            if (filterProductsDTo.ToPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filterProductsDTo.ToPrice.Value);
            }
            return query.AsQueryable();

        }

        public async Task<Product> SearchProductByNameAsync(string nameEn)
        {
            if (string.IsNullOrWhiteSpace(nameEn))
                return null;

            var trimmedName = nameEn.Trim().ToLower();

            var result = await _context.Products
                      .Include(p => p.Images)
                     .Where(p => p.NameEn.ToLower().Contains(trimmedName))
                     .FirstOrDefaultAsync();

            if (result == null)
                Console.WriteLine("[GetProductByNameEn] No matching product found.");

            return result;



        }

        public IQueryable<Product> SearchProductsByDescriptionAsync(string partialDescription)
        {
            if (string.IsNullOrWhiteSpace(partialDescription))
                return Enumerable.Empty<Product>().AsQueryable();

            string trimmedDescription = partialDescription.Trim().ToLower();

            return _context.Products.Where(
                p => p.Description != null && p.Description.Trim().ToLower().Contains(trimmedDescription)
            );

        }

        public  IQueryable<Product> SearchProductsByCompanyName(string CompanyName)
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
                return _context.Products.Where(p => false);
            string trimmedCompanyName = CompanyName.Trim().ToLower();
            var Products = _context.Products
                .Include(p => p.Company)
                .Where(p => p.Company.Name.Trim().ToLower().Contains(trimmedCompanyName)).AsQueryable();
            return Products;
        }

        public IQueryable<Product> GetProductsByCompanyId(Guid CompanyId)
        {
            if (CompanyId == Guid.Empty)
                return Enumerable.Empty<Product>().AsQueryable();
            var Products =  _context.Products.Where(p => p.CompanyId == CompanyId).AsQueryable();
            return Products;
        }

        public  IQueryable<Product> SearchProductsByCategoryName(string CategoryName)
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
                return _context.Products.Where(p => false);
            string trimmedCategoryName = CategoryName.Trim().ToLower();
            var Products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category != null && p.Category.Name.Trim().ToLower().Contains(trimmedCategoryName)).AsQueryable();
            return Products;
        }

        public IQueryable<Product> GetProductsByCategoryId(Guid CategoryId)
        {
            if(CategoryId == Guid.Empty)
                return Enumerable.Empty<Product>().AsQueryable();
            var Products = _context.Products.Where(p => p.CategoryId == CategoryId).AsQueryable();
            return Products;
        }

        public async override Task<bool> DeleteAsync(Product product)
        {
            product.IsDeleted = true;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<Domain.Entities.Product> GetAllProductsQuerable()
        {
            return _context.Products
                .Where(c => !c.IsDeleted)
                .AsNoTracking();
        }

        public IQueryable<Product> GetExpiredProducts()
        {
            var expiredProducts = _context.Products
                .Where(p => p.ExpirationDate.HasValue && p.ExpirationDate < DateTime.Now).AsQueryable();

            return expiredProducts;
        }

        public IQueryable<Product> GetUnExpiredProducts()
        {
            var UnexpiredProducts = _context.Products
                .Where(p => p.ExpirationDate.HasValue && p.ExpirationDate > DateTime.Now).AsQueryable();

            return UnexpiredProducts;
        }

        public IQueryable<Product> GetMostSelling()
        {
            var mostSellingProducts = _context.OrderItems
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
                .AsQueryable();

            return mostSellingProducts;
        }

        public IQueryable<Product> GetMorePopular()
        {
            var mostPopularProducts = _context.OrderItems
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
                .AsQueryable();

            return mostPopularProducts;
        }


        #endregion
    }
}
