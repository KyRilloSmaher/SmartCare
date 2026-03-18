using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Features.Store.Commands.Create;
using SmartCare.Application.Features.Store.Commands.Delete;
using SmartCare.Application.Features.Store.Commands.Restore;
using SmartCare.Application.Features.Store.Commands.Update;
using SmartCare.Application.Features.Store.Queries.GetAll;
using SmartCare.Application.Features.Store.Queries.GetAllForAdmin;
using SmartCare.Application.Features.Store.Queries.GetById;
using SmartCare.Application.Features.Store.Queries.GetNearest;
using SmartCare.Application.Features.Store.Queries.GetStorePharmcists;
using SmartCare.Application.Features.Store.Queries.Search;
using SmartCare.Application.Handlers.ResponseHandler;


namespace SmartCare.API.Controllers
{
    [ApiController]

    public class StoreController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StoreController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get Store By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Store.GetById)]
        [ProducesResponseType(typeof(Response<StoreResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStoreByIdAsync(Guid id)
        {
            var result = await _mediator.Send(new GetStoreByIdQuery(id));
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
            var store = await _mediator.Send(new GetNearestStoreQuery(dto));
            return ControllersHelperMethods.FinalResponse(store);
        }

        /// <summary>
        /// Search Stores By Name
        /// </summary>
        [HttpGet(ApplicationRouting.Store.SearchByName)]
        [ProducesResponseType(typeof(Response<IEnumerable<StoreResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchStoresByNameAsync([FromQuery] string name)
        {
            var result = await _mediator.Send(new SearchStoresByNameQuery(name));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Stores
        /// </summary>
        [HttpGet(ApplicationRouting.Store.GetAll)]
        [ProducesResponseType(typeof(Response<IEnumerable<StoreResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStoresAsync()
        {
            var result = await _mediator.Send(new GetAllStoresQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Store's Pharamcist
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpGet(ApplicationRouting.Store.GetStorePharmcists)]
        [ProducesResponseType(typeof(Response<IEnumerable<PharmacistResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStorePharmcistsAsync(Guid id)
        {
            var result = await _mediator.Send(new GetStorePharmcistsQuery(id));
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
            var result = await _mediator.Send(new GetAllStoresForAdminQuery());
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
            var result = await _mediator.Send(new CreateStoreCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update a Store
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPut(ApplicationRouting.Store.Update)]
        [ProducesResponseType(typeof(Response<StoreResponseForAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStoreAsync([FromBody] UpdateStoreRequestDto dto)
        {
            var result = await _mediator.Send(new UpdateStoreCommand(dto));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Restore a Store
        /// </summary>
        [Authorize(Roles = "DASHBOARD_ADMIN")]
        [HttpPatch(ApplicationRouting.Store.Restore)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestoreStoreAsync(Guid id)
        {
            var result = await _mediator.Send(new RestoreStoreCommand(id));
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
            var result = await _mediator.Send(new DeleteStoreCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
