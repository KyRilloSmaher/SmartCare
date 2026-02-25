using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Favourite.Commands;
using SmartCare.Application.CQRs.Favourite.Queries;
using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using System.Security.Claims;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class FavouritesController : ControllerBase
    {
        // private readonly IFavouriteService _favouriteService;
        private readonly IMediator _mediator;

        public FavouritesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //public FavouritesController(IFavouriteService favouriteService)
        //{
        //    _favouriteService = favouriteService;
        //}

        [HttpGet(ApplicationRouting.Favourite.GetAllForUser)]
        [ProducesResponseType(typeof(Response<List<FavoriteResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFavouritesByUser()
        {
            var userId  = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _favouriteService.GetAllFavouritesForUserAsync(userId);
            var result = await _mediator.Send(new GetAllFavouritesForUserAsyncQuery(userId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpPost(ApplicationRouting.Favourite.Create)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateFavouriteAsync(Guid productId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var Dto = new CreateFavouriteRequestDto
            {
                ClientId = userId,
                ProductId = productId
            };
            //var result = await _favouriteService.CreateFavouriteAsync(Dto);
            var result = await _mediator.Send(new CreateFavouriteAsyncCommand(Dto));
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpDelete(ApplicationRouting.Favourite.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteFavourite(Guid productId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //var result = await _favouriteService.DeleteFavouriteAsync(userId, productId);
            var result = await _mediator.Send(new DeleteFavouriteAsyncCommand(userId, productId));
            return ControllersHelperMethods.FinalResponse(result);
        }
    
    }
}
