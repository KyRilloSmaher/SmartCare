using MediatR;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Carts.Commands.AddToCart
{
    public record AddToCartCommand(AddToCartRequestDto dto) : IRequest<Response<CartItemResponseDto?>>;
}
