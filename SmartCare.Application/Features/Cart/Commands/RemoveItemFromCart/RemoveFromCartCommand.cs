using MediatR;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Carts.Commands.RemoveItemFromCart
{
    public record RemoveFromCartCommand(RemoveFromCartRequestDto dto) : IRequest<Response<bool>>;
}
