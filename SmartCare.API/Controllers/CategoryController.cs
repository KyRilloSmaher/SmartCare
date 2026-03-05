using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Category.Commands;
using SmartCare.Application.CQRs.Category.Queries;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        // private readonly ICategoryService _categoryService;
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }


        //public CategoryController(ICategoryService categoryService)
        //{
        //    _categoryService = categoryService;
        //}

        /// <summary>
        /// Get Category By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Category.GetById)]
        [ProducesResponseType(typeof(Response<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryByIdAsync(Guid id)
        {
            //var result = await _categoryService.GetCategoryByIdAsync(id);
            var result = await _mediator.Send(new GetCategoryByIdAsyncQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Search Categories By Name
        /// </summary>
        [HttpGet(ApplicationRouting.Category.SearchByName)]
        [ProducesResponseType(typeof(Response<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCategoriesByNameAsync([FromQuery]string name)
        {
            //var result = await _categoryService.SearchCategoriesByNameAsync(name);
            var result = await _mediator.Send(new SearchCategoriesByNameAsyncQuery(name));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Categories
        /// </summary>
        [HttpGet(ApplicationRouting.Category.GetAll)]
        [ProducesResponseType(typeof(Response<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoriesAsync()
        {
            //var result = await _categoryService.GetAllCategorysAsync();
            var result = await _mediator.Send(new GetAllCategorysAsyncQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Categories By Pagination
        /// </summary>
        [HttpGet(ApplicationRouting.Category.GetAllPaginated)]
        [ProducesResponseType(typeof(Response<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPaginatedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            //var result = await _categoryService.GetAllCategoriesPaginatedAsync(pageNumber, pageSize);
            var result = await _mediator.Send(new GetAllCategoriesPaginatedAsyncQuery(pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Categories (Admin)
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpGet(ApplicationRouting.Category.GetAllForAdmin)]
        [ProducesResponseType(typeof(Response<IEnumerable<CategoryResponseForAdminDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoriesForAdminAsync()
        {
            //var result = await _categoryService.GetAllCategorysForAdminAsync();
            var result = await _mediator.Send(new GetAllCategorysForAdminAsyncQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Create a New Category
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPost(ApplicationRouting.Category.Create)]
        [ProducesResponseType(typeof(Response<CategoryResponseForAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateCategoryAsync([FromForm] CreateCategoryRequestDto dto)
        {
            //var result = await _categoryService.CreateCategoryAsync(dto);
            var result = await _mediator.Send(new CreateCategoryAsyncCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update a Category
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPut(ApplicationRouting.Category.Update)]
        [ProducesResponseType(typeof(Response<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCategoryAsync(Guid id, [FromBody] UpdateCategoryRequest dto)
        {
            //var result = await _categoryService.UpdateCategoryAsync(id, dto);
            var result = await _mediator.Send(new UpdateCategoryAsyncCommand(id,dto)); 
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Change Category Logo
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPatch(ApplicationRouting.Category.ChangeImage)]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeCategoryLogoAsync(Guid id, [FromForm] ChangeCategoryLogoRequestDto dto)
        {
            //var result = await _categoryService.ChangeCategoryLogoAsync(id, dto);
            var result = await _mediator.Send(new ChangeCategoryLogoAsyncCommand(id,dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete Category
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpDelete(ApplicationRouting.Category.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCategoryAsync(Guid id)
        {
            //var result = await _categoryService.DeleteCategoryAsync(id);
            var result = await _mediator.Send(new DeleteCategoryAsyncCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
