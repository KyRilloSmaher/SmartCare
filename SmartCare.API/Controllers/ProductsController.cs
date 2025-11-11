using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.IServices;
using SmartCare.Application.Services;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Projection_Models;
using System.Linq.Expressions;

namespace SmartCare.API.Controllers
{
    [ApiController]
    //[Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _ProductService;

        public ProductsController(IProductService productService)
        {
            _ProductService = productService;
        }

        /// <summary>
        /// Get Product By Id For User
        /// </summary>
        [HttpGet(ApplicationRouting.Product.GetDetailsForUser)]
        public async Task<IActionResult> GetProductByIdForUserAsync(Guid id)
        {
            var result = await _ProductService.GetDetailsOfProductForUser(id);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Get Product By Id For Admin
        /// </summary>
        [HttpGet(ApplicationRouting.Product.GetDetailsForAdmin)]
        public async Task<IActionResult> GetProductByIdForAdminAsync(Guid id)
        {
            var result = await _ProductService.GetDetailsOfProductForAdmin(id);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Create Product
        /// </summary>
        /// <param name="ProductDto"></param>
        //[Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPost(ApplicationRouting.Product.Create)]
        public async Task<IActionResult> CreateProductAsync([FromForm] CreateProductRequestDto ProductDto)
        {
            var result = await _ProductService.CreateProductAsync(ProductDto);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Update Product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ProductDto"></param>
        //[Authorize(Roles = "Admin")]
        [HttpPut(ApplicationRouting.Product.Update)]
        public async Task<IActionResult> UpdateProductAsync(Guid id, [FromBody] UpdateProductRequestDto ProductDto)
        {
            var result = await _ProductService.UpdateProductAsync(id, ProductDto);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Delete Product
        /// </summary>
        /// <param name="ProductId"></param>
       // [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpDelete(ApplicationRouting.Product.Delete)]
        public async Task<IActionResult> DeleteProductAsync(Guid Id)
        {
            var result = await _ProductService.DeleteProductAsync(Id);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Get All Products By Pagination
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetAll)]
        public async Task<IActionResult> GetAllProductpaginationAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _ProductService.GetAllProducts(pageNumber, pageSize);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Products By Filter
        /// </summary>
        /// <param name="filterproduct"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet(ApplicationRouting.Product.GetByFilter)]
        public async Task<IActionResult> FilterProducts([FromQuery]FilterProductsDTo filterproduct,[FromQuery]int pageNumber, [FromQuery]int pageSize)
        {
            var result = await _ProductService.FilterProducts(filterproduct,pageNumber, pageSize);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Search Product By Name
        /// </summary>
        /// <param name="NameEn"></param>
        /// <returns></returns>
        [HttpGet(ApplicationRouting.Product.SearchByName)]
        public async Task<IActionResult> GetByName([FromQuery]string NameEn)
        {
            var result = await _ProductService.GetDetailsOfProductByName(NameEn);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Products By CompanyId
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetByCompanyId)]
        public async Task<IActionResult> GetBycampanyId(Guid CompanyId, int pageNumber, int pageSize)
        {
            var result = await _ProductService.GetProductsByCompanyId(CompanyId, pageNumber, pageSize);
            return ControllersHelperMethods.FinalResponse(result);

        }

        /// <summary>
        /// Search Product By CompanyName
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.SearchByCompanyName)]
        public async Task<IActionResult> SearchByCompanyName(string CompanyName, int pageNumber, int pageSize)
        {
            var result = await _ProductService.SearchProductsByCompanyName(CompanyName, pageNumber, pageSize);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Products By CategoryId
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetByCategoryId)]
        public async Task<IActionResult> GetBycategoryId(Guid CategoryId, int pageNumber, int pageSize)
        {
            var result = await _ProductService.GetProductsByCategoryId(CategoryId, pageNumber, pageSize);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Search Product By CategoryName
        /// </summary>
        /// <param name="CategoryName"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.SearchByCategoryName)]
        public async Task<IActionResult> SearchByCategoryName(string CategoryName, int pageNumber, int pageSize)
        {
            var result = await _ProductService.SearchProductsByCategoryName(CategoryName,pageNumber,pageSize);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Expired Products
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetExpired)]
        public async Task<IActionResult> GetExpiredProducts(int pageNumber, int pageSize)
        {
            var result = await _ProductService.GetexpiredProducts(pageNumber,pageSize);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Get UnExpired Products
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetUnExpired)]
        public async Task<IActionResult> GetUnExpiredProducts(int pageNumber, int pageSize)
        {
            var result = await _ProductService.GetUnexpiredProducts(pageNumber, pageSize);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Search By partialDescription
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.SearchBypartialDescription)]
        public async Task<IActionResult> SearchBypartialDescription([FromQuery] string Description, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var result = await _ProductService.SearchProductsByDescription(Description,pageNumber,pageSize);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Get Best Seller Products
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetBestSeller)]
        public async Task<IActionResult> GetBestseller(int pageNumber, int pageSize)
        {
            var result = await _ProductService.GetMostSellingProducts(pageNumber, pageSize);
            return ControllersHelperMethods.FinalResponse(result);

        }


        /// <summary>
        /// Get Best Seller Products
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetMorePopular)]
        public async Task<IActionResult> GetMorePopular(int pageNumber, int pageSize)
        {
            var result = await _ProductService.GetMorePopular(pageNumber, pageSize);
            return ControllersHelperMethods.FinalResponse(result);

        }
    }
}
