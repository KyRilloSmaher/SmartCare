using MediatR;
using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Favourite.Commands
{
    public record CreateFavouriteAsyncCommand(CreateFavouriteRequestDto Dto) : IRequest<Response<bool>>;
}
