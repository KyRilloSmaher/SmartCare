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
        IQueryable<Product> FilterProductsAsync(
       FilterProductsDTo filterProductsDTo);
        #endregion


        IQueryable<Product> GetAllProductsQuerable();


        #region Search

        #region SearchByProductName
        Task<Product> SearchProductByNameAsync(string nameEn);
        #endregion


        #region Search by Description

        IQueryable<Product> SearchProductsByDescriptionAsync(string partialDescription);
        
        #endregion


        #region SearchByCompany

        IQueryable<Product> SearchProductsByCompanyName(string CompanyName);
        IQueryable<Product> GetProductsByCompanyId(Guid CompanyId);
       
        #endregion


        #region SearchByCategory

        IQueryable<Product> SearchProductsByCategoryName(string CategoryName);
        IQueryable<Product> GetProductsByCategoryId(Guid CategoryId);

        #endregion


        IQueryable<Product> GetExpiredProducts();
        IQueryable<Product> GetUnExpiredProducts();
        IQueryable<Product> GetMostSelling();
      

        #endregion
    }
}
