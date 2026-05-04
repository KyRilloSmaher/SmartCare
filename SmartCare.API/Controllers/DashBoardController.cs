using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Pharmacist.Response;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Features.DashBoard.Commands.AddAdmin;
using SmartCare.Application.Features.DashBoard.Commands.ChangePharmacistBranch;
using SmartCare.Application.Features.DashBoard.Commands.ConfirmPharmacistEmail;
using SmartCare.Application.Features.DashBoard.Commands.Create_AssignPharamsict;
using SmartCare.Application.Features.DashBoard.Commands.RemoveAdmin;
using SmartCare.Application.Features.DashBoard.Queries.GetAllNonConfirmedPharmacist;
using SmartCare.Application.Features.DashBoard.Queries.GetLowStockProducts;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Projection_Models;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "DASHBOARD_ADMIN")]
    public class DashBoardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashBoardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(ApplicationRouting.Dashboard.LowStock)]
        [ProducesResponseType(typeof(Response<Response<PaginatedResult<LowStockProductDto>>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] GetLowStockProductsQuery query)
        {
            var result = await _mediator.Send(query);
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpGet(ApplicationRouting.Dashboard.GetNonConfirmedPharmacists)]
        [ProducesResponseType(typeof(Response<List<PharmacistProfileDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNonConfirmedPharmacistsAsync()
        {
            var result = await _mediator.Send(new GetAllNonConfirmedPharmacistQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPost(ApplicationRouting.Store.AssignPharmacist)]
        [ProducesResponseType(typeof(Response<PharmacistResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignPharmacist([FromRoute] Guid store_id, [FromForm] AssignPharmacistRequest request)
        {
            var result = await _mediator.Send(new AssignPharmacistCommand(store_id, request));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPost]
        [Route(ApplicationRouting.Dashboard.CreateAdmin)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateAdmin([FromBody] AddAdminCommand request)
        {
            var result = await _mediator.Send(request);
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPut(ApplicationRouting.Store.ChangePharmacist)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePharmacistBranch([FromRoute] string pharmacist_id, [FromRoute] Guid NewBranchId)
        {
            var command = new ChangePharmacistBranchCommand(pharmacist_id, NewBranchId);
            var result = await _mediator.Send(command);
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPut(ApplicationRouting.Dashboard.ConfirmPharmacistEmail)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmPharmacistEmailAsync([FromRoute] string id)
        {
            var command = new ConfirmPharmacistEmailCommand(id);
            var result = await _mediator.Send(command);
            return ControllersHelperMethods.FinalResponse(result);
        }


        [Route(ApplicationRouting.Dashboard.DeleteAdmin)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [HttpDelete]
        public async Task<IActionResult> DeleteAdmin([FromRoute] string id)
        {
            var command = new RemoveAdminCommand(id);
            var result = await _mediator.Send(command);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
