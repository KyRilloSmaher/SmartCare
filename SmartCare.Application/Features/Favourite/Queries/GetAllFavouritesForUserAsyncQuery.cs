using MediatR;
using Microsoft.AspNetCore.Http.Features;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Favourite.Queries
{
    public record GetAllFavouritesForUserAsyncQuery(string userId) : IRequest<Response<IEnumerable<FavoriteResponseDto>>>;
}
