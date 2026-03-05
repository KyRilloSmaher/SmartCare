using MediatR;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;


namespace SmartCare.Application.Features.Orders.Commands.CreatePickUpOrder
{
    public record CreatePickupOrderFromCartCommand(string clientId, CreatePickUpOrderRequestDto dto) : IRequest<Response<PickUpOrderResponseDto?>>;
}
