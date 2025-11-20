
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SmartCare.API.Helpers;
    using SmartCare.Application.DTOs.Address.Requests;
    using SmartCare.Application.DTOs.Address.Responses;
    using SmartCare.Application.Handlers.ResponseHandler;
    using SmartCare.Application.IServices;
    using System.Security.Claims;

    namespace SmartCare.API.Controllers
    {
        [ApiController]
        [Authorize]
        public class ClientAddressController : ControllerBase
        {
            private readonly IAddressService _addressService;

            public ClientAddressController(IAddressService addressService)
            {
                _addressService = addressService;
            }
        /// <summary>
        /// Get all addresses for logged-in client
        /// </summary>
        [HttpGet(ApplicationRouting.ClientAddress.GetAll)]
        [ProducesResponseType(typeof(Response<IEnumerable<AddressResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientAddressesAsync()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.GetClientAddressesAsync(clientId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Add new address for logged-in client
        /// </summary>
        [HttpPost(ApplicationRouting.ClientAddress.Add)]
        [ProducesResponseType(typeof(Response<AddressResponseDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddNewClientAddressAsync(CreateAddressRequestDto dto)
         {
                var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _addressService.AddNewClientAddressAsync(clientId, dto);
                return ControllersHelperMethods.FinalResponse(result);
         }

        /// <summary>
        /// Update an existing address for logged-in client
        /// </summary>
        [HttpPut(ApplicationRouting.ClientAddress.Update)]
        [ProducesResponseType(typeof(Response<AddressResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateClientAddressAsync(UpdateAddressRequestDto dto)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.UpdateClientAddressAsync(clientId, dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Set a client address as primary
        /// </summary>
        [HttpPatch(ApplicationRouting.ClientAddress.SetAsPrimary)]
        [ProducesResponseType(typeof(Response<AddressResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetAddressAsPrimaryAddressAsync([FromRoute] Guid addressId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.SetAddressAsPrimaryAddressAsync(clientId, addressId);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Delete address for logged-in client
        /// </summary>
        [HttpDelete(ApplicationRouting.ClientAddress.Delete)]
            [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
            public async Task<IActionResult> DeleteClientAddressAsync([FromRoute]Guid addressId)
            {
                var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _addressService.DeleteClientAddressAsync(clientId, addressId);
                return ControllersHelperMethods.FinalResponse(result);
            }

        }
    }


