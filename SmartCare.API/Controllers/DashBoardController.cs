using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.Features.DashBoard.Commands.ChangePharmacistBranch;
using SmartCare.Application.Features.DashBoard.Commands.Create_AssignPharamsict;

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

        [HttpPost(ApplicationRouting.Store.AssignPharmacist)]
        public async Task<IActionResult> AssignPharmacist([FromRoute] Guid store_id, [FromForm] AssignPharmacistRequest request)
        {
            var result = await _mediator.Send(new AssignPharmacistCommand(store_id, request));
            return ControllersHelperMethods.FinalResponse(result);
        }
        [HttpPut(ApplicationRouting.Store.ChangePharmacist)]
        public async Task<IActionResult> ChangePharmacistBranch([FromRoute] string pharmacist_id, [FromRoute] Guid NewBranchId)
        {
            var command = new ChangePharmacistBranchCommand(pharmacist_id, NewBranchId);
            var result = await _mediator.Send(command);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
