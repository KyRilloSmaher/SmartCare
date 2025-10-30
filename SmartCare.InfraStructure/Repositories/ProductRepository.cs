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
using static SmartCare.API.Helpers.ApplicationRouting;

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
        public async Task<List<Product>> FilterProductsAsync(FilterProductsDTo filterProductsDTo, Expression<Func<Product, object>> orderBy, bool ascending = true)
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

            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return await query.ToListAsync();

        }

        public async Task<Product> SearchProductByNameAsync(string? nameAr, string? nameEn, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(nameAr) && string.IsNullOrWhiteSpace(nameEn))
                return  null;
            var query = _context.Products.AsQueryable();

            query = query.Where(p =>
                (!string.IsNullOrWhiteSpace(nameAr) && p.NameAr.ToLower() == nameAr.Trim().ToLower()) ||
                (!string.IsNullOrWhiteSpace(nameEn) && p.NameEn.ToLower() == nameEn.Trim().ToLower()));

            query = ascending
                ? query.OrderBy(p => p.NameAr).ThenBy(p => p.NameEn)
                : query.OrderByDescending(p => p.NameAr).ThenByDescending(p => p.NameEn);

            return await query.FirstOrDefaultAsync();


        }

        public async Task<List<Product>> SearchProductsByDescriptionAsync(string partialDescription)
        {
            if(partialDescription == null)
                return new List<Product>();
            return await _context.Products.Where(
                p => p.Description.ToLower().Contains(partialDescription.ToLower())
                ).ToListAsync();
        }

        public async Task<List<Product>> SearchProductsByCompanyName(string CompanyName)
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
                return new List<Product>();
            var Company = await _context.Companies.FirstOrDefaultAsync(c => c.Name.ToLower() == CompanyName.Trim().ToLower());
            if (Company == null)
                return new List<Product>();
            var Products = await _context.Products.Where(p => p.CompanyId == Company.Id).ToListAsync();
            return Products;
        }

        public async Task<List<Product>> SearchProductsByCompanyId(Guid CompanyId)
        {
            if (CompanyId == Guid.Empty)
                return new List<Product>();
            var Products = await _context.Products.Where(p => p.CompanyId == CompanyId).ToListAsync();
            return Products;
        }

        public async Task<List<Product>> SearchProductsByCategoryName(string CategoryName)
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
                return new List<Product>();
            var Category = _context.Categories.FirstOrDefault(c => c.Name.ToLower() == CategoryName.ToLower());
            if(Category == null)
                return new List<Product>();
            var Products = await _context.Products.Where(p => p.CategoryId == Category.Id).ToListAsync();
            return Products;
        }

        public async Task<List<Product>> SearchProductsByCategoryId(Guid CategoryId)
        {
            if(CategoryId == Guid.Empty)
                return new List<Product>();
            var Products = await _context.Products.Where(p => p.CategoryId == CategoryId).ToListAsync();
            return Products;
        }
        #endregion
    }
}
