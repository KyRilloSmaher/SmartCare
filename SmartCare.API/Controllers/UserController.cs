using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
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
        private readonly IClientService _clientService;

        public UserController(IClientService clientService)
        {
            _clientService = clientService;
        }
        /// <summary>
        /// Get A Client By Id
        /// </summary>
        [HttpGet(ApplicationRouting.Client.GetClientById)]
        [ProducesResponseType(typeof(Response<ClientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientByIdAsync(string id)
        {
            var result = await _clientService.GetClientByIdAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get A Client By Email
        /// </summary>
        
        [HttpGet(ApplicationRouting.Client.GetClientByEmail)]
        [ProducesResponseType(typeof(Response<ClientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientByEmailAsync(string email)
        {
            var result = await _clientService.GetClientByEmailAsync(email);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get All Clients 
        /// </summary>
        [HttpGet(ApplicationRouting.Client.GetAllClients)]
        [ProducesResponseType(typeof(Response<IEnumerable<ClientResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClientsAsync()
        {
            var result = await _clientService.GetAllClientsAsync();
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Update A Client
        /// </summary>

        [HttpPut(ApplicationRouting.Client.UpdateClient)]
        [ProducesResponseType(typeof(Response<ClientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateClientProfileAsync(UpdateClientRequest dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _clientService.UpdateClientAsync(userId , dto);
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
            var result = await _clientService.ChangeClientProfileImageAsync(userId, dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete A Client
        /// </summary>

        [HttpDelete(ApplicationRouting.Client.DeleteClient)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteClientAsync(string id)
        {
            var result = await _clientService.DeleteClientAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

    }
}
