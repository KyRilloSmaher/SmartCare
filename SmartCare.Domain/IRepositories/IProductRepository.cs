using SmartCare.Domain.Entities;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {

        #region FilterProducts
        Task<List<Product>> FilterProductsAsync(
       FilterProductsDTo filterProductsDTo,
       Expression<Func<Product, object>> orderBy,
       bool ascending = true);
        #endregion


        #region Search

        #region SearchByProductName
        Task<Product> SearchProductByNameAsync(
          string? nameAr,
          string? nameEn,
          bool ascending = true);
        #endregion


        #region Search by Description

        Task<List<Product>> SearchProductsByDescriptionAsync(string partialDescription);
        
        #endregion


        #region SearchByCompany

        Task<List<Product>> SearchProductsByCompanyName(string CompanyName);
        Task<List<Product>> SearchProductsByCompanyId(Guid CompanyId);
       
        #endregion


        #region SearchByCategory

        Task<List<Product>> SearchProductsByCategoryName(string CategoryName);
        Task<List<Product>> SearchProductsByCategoryId(Guid CategoryId);
        
        #endregion


        #endregion
    }
}
