using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Product.Queries;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Features.Product.Commands.Create;
using SmartCare.Application.Features.Product.Commands.Delete;
using SmartCare.Application.Features.Product.Commands.Restore;
using SmartCare.Application.Features.Product.Commands.Update;
using SmartCare.Application.Features.Product.Queries.GetContradictionsFromUserHistory;
using SmartCare.Application.Features.Product.Queries.RecommendSimilarProducts;
using SmartCare.Application.Features.Product.Queries.SearchInProducts;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;

using SmartCare.Domain.Projection_Models;
using System.Security.Claims;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        //private readonly IProductService _ProductService;
        private readonly IMediator _mediator;
        private readonly IResponseHandler _responseHandler;

        //public ProductsController(IProductService productService)
        //{
        //    _ProductService = productService;
        //}

        public ProductsController(IMediator mediator, IResponseHandler responseHandler)
        {
            _mediator = mediator;
            _responseHandler = responseHandler;
        }

        /// <summary>
        /// Get Product By Id For User
        /// </summary>
        [HttpGet(ApplicationRouting.Product.GetDetailsForUser)]
        [ProducesResponseType(typeof(Response<ProductResponseDtoForClient>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductByIdForUserAsync(Guid id)
        {
            //var result = await _ProductService.GetDetailsOfProductForUser(id);
            var result = await _mediator.Send(new GetDetailsOfProductForUserQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Get Product By Id For Admin
        /// </summary>
        [HttpGet(ApplicationRouting.Product.GetDetailsForAdmin)]
        [ProducesResponseType(typeof(Response<ProductResponseDtoForAdmin>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductByIdForAdminAsync(Guid id)
        {
            //var result = await _ProductService.GetDetailsOfProductForAdmin(id);
            var result = await _mediator.Send(new GetDetailsOfProductForAdminQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Get All Products By Pagination
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetAll)]
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProductpaginationAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            //var result = await _ProductService.GetAllProducts(pageNumber, pageSize);
            var result = await _mediator.Send(new GetAllProductsQuery(pageNumber, pageSize));
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
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FilterProducts([FromQuery] FilterProductsDTo filterproduct, [FromQuery] int pageNumber =1, [FromQuery] int pageSize =10)
        {
            //var result = await _ProductService.FilterProducts(filterproduct, pageNumber, pageSize);
            var result = await _mediator.Send(new FilterProductsQuery(filterproduct, pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Search Product By Name
        /// </summary>
        /// <param name="NameEn"></param>
        /// <returns></returns>
        [HttpGet(ApplicationRouting.Product.SearchByName)]
        [ProducesResponseType(typeof(Response<ProductResponseDtoForClient>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByName([FromQuery] string NameEn)
        {
            //var result = await _ProductService.GetDetailsOfProductByName(NameEn);
            var result = await _mediator.Send(new GetDetailsOfProductByNameQuery(NameEn));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Products By CompanyId
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetByCompanyId)]
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBycampanyId(Guid CompanyId, int pageNumber, int pageSize)
        {
            //var result = await _ProductService.GetProductsByCompanyId(CompanyId, pageNumber, pageSize);
            var result = await _mediator.Send(new GetProductsByCompanyIdQuery(CompanyId, pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);

        }

        /// <summary>
        /// Search Product By CompanyName
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.SearchByCompanyName)]
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByCompanyName(string CompanyName, int pageNumber, int pageSize)
        {
            //var result = await _ProductService.SearchProductsByCompanyName(CompanyName, pageNumber, pageSize);
            var result = await _mediator.Send(new SearchProductsByCompanyNameQuery(CompanyName, pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Products By CategoryId
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetByCategoryId)]
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBycategoryId(Guid CategoryId, int pageNumber, int pageSize)
        {
            //var result = await _ProductService.GetProductsByCategoryId(CategoryId, pageNumber, pageSize);
            var result = await _mediator.Send(new GetProductsByCategoryIdQuery(CategoryId, pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Search Product By CategoryName
        /// </summary>
        /// <param name="CategoryName"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.SearchByCategoryName)]
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByCategoryName(string CategoryName, int pageNumber, int pageSize)
        {
            //var result = await _ProductService.SearchProductsByCategoryName(CategoryName, pageNumber, pageSize);
            var result = await _mediator.Send(new SearchProductsByCategoryNameQuery(CategoryName, pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Search By partialDescription
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.SearchBypartialDescription)]
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchBypartialDescription([FromQuery] string Description, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            //var result = await _ProductService.SearchProductsByDescription(Description, pageNumber, pageSize);
            var result = await _mediator.Send(new SearchProductsByDescriptionQuery(Description, pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Get Best Seller Products
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetBestSeller)]
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBestseller(int pageNumber, int pageSize)
        {
            //var result = await _ProductService.GetMostSellingProducts(pageNumber, pageSize);
            var result = await _mediator.Send(new GetMostSellingProductsQuery(pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);

        }

        /// <summary>
        /// Search In Products Using Both Semantic And 
        /// </summary>
        /// <param name="query"></param>
        [HttpGet(ApplicationRouting.Product.Search)]
        [ProducesResponseType(typeof(Response<ICollection<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery]string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return ControllersHelperMethods.FinalResponse(_responseHandler.BadRequest("Query Is Required"));
            }
            var result = await _mediator.Send(new SearchQuery(query));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// 
        /// Recommend Similar Products
        /// </summary>
        /// <param name="productId">ProductId</param>
        [HttpGet(ApplicationRouting.Product.RecommendSimilars)]
        [ProducesResponseType(typeof(Response<ICollection<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecommendSimilarProdcurts(Guid id)
        {
            
            var result = await _mediator.Send(new RecommendSimilarProductsQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get The Conradiction Products By A Product From Client History
        /// </summary>
        /// <param name="Id">ProductId</param>
        [HttpGet(ApplicationRouting.Product.GetContradictions)]
        [ProducesResponseType(typeof(Response<ICollection<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContradictions( Guid id)
        {
            var clientId = User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _mediator.Send(new GetContradictionsFromUserHistoryQuery(clientId,id));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Best Seller Products
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        [HttpGet(ApplicationRouting.Product.GetMorePopular)]
        [ProducesResponseType(typeof(Response<PaginatedResult<ProductResponseDtoForClient>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMorePopular(int pageNumber, int pageSize)
        {
            //var result = await _ProductService.GetMorePopular(pageNumber, pageSize);
            var result = await _mediator.Send(new GetMorePopularQuery(pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);

        }



        /// <summary>
        /// Create Product
        /// </summary>
        /// <param name="ProductDto"></param>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPost(ApplicationRouting.Product.Create)]
        [ProducesResponseType(typeof(Response<ProductResponseDtoForAdmin>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateProductAsync([FromForm] CreateProductRequestDto ProductDto)
        {
            var result = await _mediator.Send(new CreateProductCommand(ProductDto));
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Update Product
        /// </summary>
        /// <param name="id"></param>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPatch(ApplicationRouting.Product.Restore)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestoreProductAsync(Guid id)
        {
            var result = await _mediator.Send(new RestoreProductCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Update Product
        /// </summary>
        /// <param name="ProductDto"></param>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPut(ApplicationRouting.Product.Update)]
        [ProducesResponseType(typeof(Response<ProductResponseDtoForAdmin>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProductAsync([FromForm] UpdateProductRequestDto ProductDto)
        {
            var result = await _mediator.Send(new UpdateProductCommand(ProductDto));
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Delete Product
        /// </summary>
        /// <param name="ProductId"></param>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpDelete(ApplicationRouting.Product.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteProductAsync(Guid id)
        {
            var result = await _mediator.Send(new DeleteProductCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }

    }
}
