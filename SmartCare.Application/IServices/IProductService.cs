using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IProductService
    {
         Task<Response<ProductResponseDtoForAdmin>> CreateProductAsync(CreateProductRequestDto ProductDto);
         Task<Response<ProductResponseDtoForAdmin>> UpdateProductAsync(Guid Id, UpdateProductRequestDto ProductDto);
         Task<Response<bool>> DeleteProductAsync(Guid Id);
        Task<Response<ProductResponseDtoForClient>> GetDetailsOfProductForUser(Guid productId);
        Task<Response<ProductResponseDtoForAdmin>> GetDetailsOfProductForAdmin(Guid productId);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> FilterProducts(FilterProductsDTo filterproduct, string orderBy,bool isAsending ,int pageNumber, int pageSize);
        Task<Response<ProductResponseDtoForClient>> GetDetailsOfProductByName(string NameEn);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetAllProducts(int pageNumber, int pageSize);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetProductsByCompanyId(Guid CompanyId ,int pageNumber, int pageSize);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> SearchProductsByCompanyName(string CompanyName ,int pageNumber, int pageSize);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetProductsByCategoryId(Guid CategoryId, int pageNumber, int pageSize);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> SearchProductsByCategoryName(string CategoryName, int pageNumber, int pageSize);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetexpiredProducts(int pageNumber, int pageSize);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetUnexpiredProducts(int pageNumber, int pageSize);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> GetMostSellingProducts(int pageNumber, int pageSize);
        Task<Response<PaginatedResult<ProductResponseDtoForClient>>> SearchProductsByDescription(string Description, int pageNumber, int pageSize);
    }
}
