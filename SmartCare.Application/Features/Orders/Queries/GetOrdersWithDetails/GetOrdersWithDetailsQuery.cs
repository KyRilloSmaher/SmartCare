using MediatR;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;

namespace SmartCare.Application.Features.Orders.Queries.GetOrdersWithDetails
{
     public record GetOrdersWithDetailsQuery(GetOrdersForAdminRequestDto Request) : IRequest<Response<PaginatedResult<OrderResponseDto>>>;
}
