using MediatR;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Orders.CreateOnlineOrder
{
    public record CreateOnlineOrderFromCartAsyncCommand(string clientId, CreateOnlineOrderRequestDto dto) : IRequest<Response<OrderResponseDto?>>;
}
