using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Client.Commands;
using SmartCare.Application.CQRs.Client.Queries;
using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using System.Security.Claims;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        //private readonly IClientService _clientService;
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }


        //public UserController(IClientService clientService)
        //{
        //    _clientService = clientService;
        //}
        /// <summary>
        /// Get A Client By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Client.GetById)]
        [ProducesResponseType(typeof(Response<ClientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientByIdAsync(string id)
        {
           // var result = await _clientService.GetClientByIdAsync(id);
           var result = await _mediator.Send(new GetClientByIdAsyncQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get A Client By Email
        /// </summary>
        
        [HttpGet(ApplicationRouting.Client.GetByEmail)]
        [ProducesResponseType(typeof(Response<ClientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientByEmailAsync(string email)
        {
            //var result = await _clientService.GetClientByEmailAsync(email);
            var result = await _mediator.Send(new GetClientByEmailAsyncQuery(email));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Clients 
        /// </summary>
        [HttpGet(ApplicationRouting.Client.GetAll)]
        [ProducesResponseType(typeof(Response<IEnumerable<ClientResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClientsAsync()
        {
           // var result = await _clientService.GetAllClientsAsync();
           var result = await _mediator.Send(new GetAllClientsAsyncQuery());
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update A Client
        /// </summary>

        [HttpPatch(ApplicationRouting.Client.UpdateProfile)]
        [ProducesResponseType(typeof(Response<ClientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateClientProfileAsync(UpdateClientRequest dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _clientService.UpdateClientAsync(userId , dto);
            var result = await _mediator.Send(new UpdateClientAsyncCommand(userId , dto));
            return ControllersHelperMethods.FinalResponse(result);
        }
        /// <summary>
        /// Change Profile Image for Client
        /// </summary>
        [HttpPut(ApplicationRouting.Client.ChangeProfileImage)]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeClientProfileImageAsync(ChangeClientProfileImageRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _clientService.ChangeClientProfileImageAsync(userId, dto);
            var result = await _mediator.Send(new ChangeClientProfileImageAsyncCommand(userId , dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete A Client
        /// </summary>

        [HttpDelete(ApplicationRouting.Client.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteClientAsync(string id)
        {
            //var result = await _clientService.DeleteClientAsync(id);
            var result = await _mediator.Send(new DeleteClientAsyncCommand(id));
            return ControllersHelperMethods.FinalResponse(result);
        }

    }
}
