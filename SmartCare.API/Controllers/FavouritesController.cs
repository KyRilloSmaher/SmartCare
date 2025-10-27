using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.IServices;
using SmartCare.Application.Services;
using System.Security.Claims;

namespace SmartCare.API.Controllers
{
    [ApiController]
    public class FavouritesController : ControllerBase
    {
        private readonly IFavouriteService _favouriteService;

        public FavouritesController(IFavouriteService favouriteService)
        {
            _favouriteService = favouriteService;
        }

        [HttpGet(ApplicationRouting.Favourite.GetAllForUser)]
        public async Task<IActionResult> GetFavouritesByUser()
        {
            var user  = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _favouriteService.GetAllFavouritesForUserAsync(user);
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpPost(ApplicationRouting.Favourite.Create)]
        public async Task<IActionResult> CreateFavouriteAsync(CreateFavouriteRequestDto Dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _favouriteService.CreateFavouriteAsync(userId, Dto);
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpDelete(ApplicationRouting.Favourite.Delete)]
        public async Task<IActionResult> DeleteFavourite(Guid Id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _favouriteService.DeleteFavouriteAsync(userId, Id);
            return ControllersHelperMethods.FinalResponse(result);
        }
    
    }
}
