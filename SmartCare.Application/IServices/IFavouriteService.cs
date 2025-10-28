using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IFavouriteService
    {
        Task<Response<FavoriteResponseDto>> CreateFavouriteAsync(CreateFavouriteRequestDto Dto);
        Task<Response<bool>> DeleteFavouriteAsync(string userId, Guid Id);
        Task<Response<IEnumerable<FavoriteResponseDto>>> GetAllFavouritesForUserAsync(string userId);
    }
}
