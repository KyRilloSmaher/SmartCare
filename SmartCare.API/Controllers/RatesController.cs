using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using System.Security.Claims;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class RatesController : ControllerBase
    {
        private readonly IRateService _ratesService;
        public RatesController(IRateService ratesService)
        {
            _ratesService = ratesService;
        }
        /// <summary>
        /// Get Rate By Id
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpGet(ApplicationRouting.Rate.GetRateById)]
        public async Task<IActionResult> GetRateByIdAsync(Guid id)
        {
            var result = await _ratesService.GetRateByIdAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Rates By User
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpGet(ApplicationRouting.Rate.GetAllRatesForUser)]
        public async Task<IActionResult> GetRatesbyUserAsync()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _ratesService.GetAllRatesForUserAsync(userId);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Rates For Product
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpGet(ApplicationRouting.Rate.GetAllRateForProduct)]
        public async Task<IActionResult> GetProductRatesAsync(Guid id)
        {
            var result = await _ratesService.GetAllRatesForProductAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Create rate by User
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpPost(ApplicationRouting.Rate.CreateRate)]
        public async Task<IActionResult> CreateRateAsync(CreateRateRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _ratesService.CreateRateAsync(userId, dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        ///Update Rate By Id
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpPut(ApplicationRouting.Rate.UpdateRate)]
        public async Task<IActionResult> UpdateRateAsync(UpdateRateRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _ratesService.UpdateRateAsync(userId ,dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete Rate By Id
        /// </summary>
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [HttpDelete(ApplicationRouting.Rate.DeleteRate)]
        public async Task<IActionResult> DeleteRateAsync(Guid id)
        {
            var result = await _ratesService.DeleteRateAsync(id);
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
