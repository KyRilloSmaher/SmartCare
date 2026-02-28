using MediatR;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Carts.Queries.GetUserActiveCart
{
    public record GetUserActiveCartQuery(string userId) : IRequest<Response<CartResponseDto>>;
}
