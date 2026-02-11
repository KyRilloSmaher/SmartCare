using MediatR;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Cart.Commands
{
    public record AddToCartAsyncCommand(AddToCartRequestDto dto) : IRequest<Response<CartItemResponseDto?>>;
}
