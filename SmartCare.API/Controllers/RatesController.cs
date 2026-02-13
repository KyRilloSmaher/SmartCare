using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Rate.Commands;
using SmartCare.Application.CQRs.Rate.Queries;
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
        //private readonly IRateService _ratesService;
        private readonly IMediator _mediator;

        public RatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //public RatesController(IRateService ratesService)
        //{
        //    _ratesService = ratesService;
        //}
        /// <summary>
        /// Get Rate By Id
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpGet(ApplicationRouting.Rate.GetById)]
        public async Task<IActionResult> GetRateByIdAsync(Guid id)
        {
            //var result = await _ratesService.GetRateByIdAsync(id);
            var result = await _mediator.Send(new GetRateByIdAsyncQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Rates By User
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpGet(ApplicationRouting.Rate.GetAllForUser)]
        public async Task<IActionResult> GetRatesbyUserAsync()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _ratesService.GetAllRatesForUserAsync(userId);
            var result = await _mediator.Send(new GetAllRatesForUserAsyncQuery(userId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Get Rates For Product
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpGet(ApplicationRouting.Rate.GetAllForProduct)]
        public async Task<IActionResult> GetProductRatesAsync(Guid id)
        {
            //var result = await _ratesService.GetAllRatesForProductAsync(id);
            var result = await _mediator.Send(new GetAllRatesForProductAsyncQuery(id));
            return ControllersHelperMethods.FinalResponse(result);
        }


        /// <summary>
        /// Create rate by User
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpPost(ApplicationRouting.Rate.Create)]
        public async Task<IActionResult> CreateRateAsync(CreateRateRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _ratesService.CreateRateAsync(userId, dto);
            var result = await _mediator.Send(new CreateRateAsyncCommand(userId, dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        ///Update Rate By Id
        /// </summary>
        [ProducesResponseType(typeof(Response<RateResponseDto>), StatusCodes.Status200OK)]
        [HttpPut(ApplicationRouting.Rate.Update)]
        public async Task<IActionResult> UpdateRateAsync(UpdateRateRequestDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _ratesService.UpdateRateAsync(userId ,dto);
            var result = await _mediator.Send(new UpdateRateAsyncCommand(userId, dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        /// <summary>
        /// Delete Rate By Id
        /// </summary>
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [HttpDelete(ApplicationRouting.Rate.Delete)]
        public async Task<IActionResult> DeleteRateAsync(Guid id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _ratesService.DeleteRateAsync(userId,id);
            var result = await _mediator.Send(new DeleteRateAsyncCommand(userId, id));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
