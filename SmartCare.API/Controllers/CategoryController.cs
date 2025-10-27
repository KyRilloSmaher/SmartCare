using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
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
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get Category By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Category.GetById)]
        [ProducesResponseType(typeof(Response<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryByIdAsync(Guid id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Search Categories By Name
        /// </summary>
        [HttpGet(ApplicationRouting.Category.SearchByName)]
        [ProducesResponseType(typeof(Response<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCategoriesByNameAsync([FromQuery]string name)
        {
            var result = await _categoryService.SearchCategoriesByNameAsync(name);
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Get All Categories
        /// </summary>
        [HttpGet(ApplicationRouting.Category.GetAll)]
        [ProducesResponseType(typeof(Response<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoriesAsync()
        {
            var result = await _categoryService.GetAllCategorysAsync();
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
            var result = await _categoryService.GetAllCategorysForAdminAsync();
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
            var result = await _categoryService.CreateCategoryAsync(dto);
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
            var result = await _categoryService.UpdateCategoryAsync(id, dto);
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
            var result = await _categoryService.ChangeCategoryLogoAsync(id, dto);
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
            var result = await _categoryService.DeleteCategoryAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
