using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Application.Handlers.ResponsesHandler;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        /// <summary>
        /// Get Store By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Store.GetStoreById)]
        [ProducesResponseType(typeof(Response<StoreResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStoreByIdAsync(Guid id)
        {
            var result = await _storeService.GetStoreByIdAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Nearest Store By Coordinates
        /// </summary>
        [HttpGet(ApplicationRouting.Store.GetNearestStore)]
        [ProducesResponseType(typeof(Response<StoreResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNearestStoreAsync([FromQuery] AddressValuesDto dto)
        {
            var store = await _storeService.GetNearestStoreAsync(dto);
            return ControllersHelperMethods.FinalResponse(store);
        }

        /// <summary>
        /// Search Stores By Name
        /// </summary>
        [HttpGet(ApplicationRouting.Store.SearchStoresByName)]
        [ProducesResponseType(typeof(Response<IEnumerable<StoreResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchStoresByNameAsync([FromQuery] string name)
        {
            var result = await _storeService.SearchStoresByNameAsync(name);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Stores
        /// </summary>
        [HttpGet(ApplicationRouting.Store.GetAllStores)]
        [ProducesResponseType(typeof(Response<IEnumerable<StoreResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStoresAsync()
        {
            var result = await _storeService.GetAllStoresAsync();
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Stores (Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet(ApplicationRouting.Store.GetAllStoresForAdmin)]
        [ProducesResponseType(typeof(Response<IEnumerable<StoreResponseForAdminDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStoresForAdminAsync()
        {
            var result = await _storeService.GetAllStoresForAdminAsync();
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Create a New Store
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost(ApplicationRouting.Store.CreateStore)]
        [ProducesResponseType(typeof(Response<StoreResponseForAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateStoreAsync([FromBody] CreateStoreRequestDto dto)
        {
            var result = await _storeService.CreateStoreAsync(dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update a Store
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut(ApplicationRouting.Store.UpdateStore)]
        [ProducesResponseType(typeof(Response<StoreResponseForAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStoreAsync(Guid id, [FromBody] UpdateStoreRequestDto dto)
        {
            var result = await _storeService.UpdateStoreAsync(id, dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete Store
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete(ApplicationRouting.Store.DeleteStore)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteStoreAsync(Guid id)
        {
            var result = await _storeService.DeleteStoreAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
