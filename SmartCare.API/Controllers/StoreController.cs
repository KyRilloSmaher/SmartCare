using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Store.Commands;
using SmartCare.Application.CQRs.Store.Queries;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;

namespace SmartCare.API.Controllers
{
    [ApiController]

    public class StoreController : ControllerBase
    {
        //private readonly IStoreService _storeService;
        private readonly IMediator _mediator;

        public StoreController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //public StoreController(IStoreService storeService)
        //{
        //    _storeService = storeService;
        //}

        /// <summary>
        /// Get Store By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Store.GetById)]
        [ProducesResponseType(typeof(Response<StoreResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStoreByIdAsync(Guid id)
        {
            //var result = await _storeService.GetStoreByIdAsync(id);
            var result = await _mediator.Send(new GetStoreByIdAsyncQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Nearest Store By Coordinates
        /// </summary>
        [HttpGet(ApplicationRouting.Store.GetNearest)]
        [ProducesResponseType(typeof(Response<StoreResponseDto>), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> GetNearestStoreAsync([FromQuery] AddressValuesDto dto)
        {
            //var store = await _storeService.GetNearestStoreAsync(dto);
            var store = await _mediator.Send(new GetNearestStoreAsyncQuery(dto));
            return ControllersHelperMethods.FinalResponse(store);
        }

        /// <summary>
        /// Search Stores By Name
        /// </summary>
        [HttpGet(ApplicationRouting.Store.SearchByName)]
        [ProducesResponseType(typeof(Response<IEnumerable<StoreResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchStoresByNameAsync([FromQuery] string name)
        {
            //var result = await _storeService.SearchStoresByNameAsync(name);
            var result = await _mediator.Send(new SearchStoresByNameAsyncQuery(name));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Stores
        /// </summary>
        [HttpGet(ApplicationRouting.Store.GetAll)]
        [ProducesResponseType(typeof(Response<IEnumerable<StoreResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStoresAsync()
        {
            //var result = await _storeService.GetAllStoresAsync();
            var result = await _mediator.Send(new GetAllStoresAsyncQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Stores (Admin)
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpGet(ApplicationRouting.Store.GetAllForAdmin)]
        [ProducesResponseType(typeof(Response<IEnumerable<StoreResponseForAdminDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStoresForAdminAsync()
        {
            //var result = await _storeService.GetAllStoresForAdminAsync();
            var result = await _mediator.Send(new GetAllStoresForAdminAsyncQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Create a New Store
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPost(ApplicationRouting.Store.Create)]
        [ProducesResponseType(typeof(Response<StoreResponseForAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateStoreAsync([FromBody] CreateStoreRequestDto dto)
        {
            //var result = await _storeService.CreateStoreAsync(dto);
            var result = await _mediator.Send(new CreateStoreAsyncCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update a Store
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPut(ApplicationRouting.Store.Update)]
        [ProducesResponseType(typeof(Response<StoreResponseForAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStoreAsync(Guid id, [FromBody] UpdateStoreRequestDto dto)
        {
            //var result = await _storeService.UpdateStoreAsync(id, dto);
            var result = await _mediator.Send(new UpdateStoreAsyncCommand(id, dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete Store
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpDelete(ApplicationRouting.Store.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteStoreAsync(Guid id)
        {
            //var result = await _storeService.DeleteStoreAsync(id);
            var result = await _mediator.Send(new DeleteStoreAsyncCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
